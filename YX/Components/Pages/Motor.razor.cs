using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Microsoft.EntityFrameworkCore;
using MathNet.Numerics;

public partial class MotorBase : ComponentBase
{
    [Inject] public IJSRuntime JS { get; set; } = default!;
    [Inject] public YX.Services.MotorManager MotorManager { get; set; } = default!;
    [Inject] public YX.Services.MotorCalculator MotorCalculator { get; set; } = default!;
    [Inject] public YX.Services.MotorValidator MotorValidator { get; set; } = default!;
    [Inject] public YX.Services.NotificationService Notifications { get; set; } = default!;

    // 列表从数据库加载
    protected List<MotorModel> SavedMotors { get; set; } = new List<MotorModel>();
    protected MotorModel EditingMotor { get; set; } = new MotorModel();

    protected override async Task OnInitializedAsync()
    {
        // load persisted motors from SQLite via MotorManager
        SavedMotors = await MotorManager.GetAllMotorsAsync();
        SelectedIndex = -1;
        editContext = new EditContext(EditingMotor);
    }

    // 选项改变
    protected int SelectedIndex { get; set; } = -1;
    protected async Task OnSelectChanged(ChangeEventArgs e)
    {
        if (!int.TryParse(e?.Value?.ToString(), out var idx)) idx = -1;
        SelectedIndex = idx;
        await LoadSelectedAsync();
    }

    // 改变后数据变更
    async Task LoadSelectedAsync()
    {
        if (SelectedIndex >= 0 && SelectedIndex < SavedMotors.Count)
        {
            var src = SavedMotors[SelectedIndex];
            var (m, pts) = await MotorManager.GetMotorWithPointsAsync(src.Id);
            if (m != null)
            {
                EditingMotor = new MotorModel
                {
                    Id = m.Id,
                    MotorName = m.MotorName,
                    MotorType = m.MotorType,
                    Voltage = m.Voltage,
                    MotorEfficiency = m.MotorEfficiency,
                    MaxEfficiencyLoadRatio = m.MaxEfficiencyLoadRatio,
                    NoLoadPoint = m.NoLoadPoint,
                    StallPoint = m.StallPoint
                };
            }
            EditingPoints = pts.Select(d => new MotorDataPoint
            {
                Id = d.Id,
                MotorId = d.MotorId,
                Torque = d.Torque,
                Speed = d.Speed,
                Current = d.Current
            }).ToList();
        }
        else
        {
            EditingMotor = new MotorModel();
            EditingPoints = new List<MotorDataPoint>();
        }
        // reset EditContext for new editing instance
        editContext = new EditContext(EditingMotor);
        ValidationErrors.Clear(); PageAlertMessage = null;
        StateHasChanged();
    }

    // 编辑时的数据点（绑定表格）
    protected List<MotorDataPoint> EditingPoints { get; set; } = new List<MotorDataPoint>();

    // 拟合结果
    double[] CurrentCoeffs = Array.Empty<double>();
    double[] SpeedCoeffs = Array.Empty<double>();
    protected bool HasFit { get; set; } = false;
    double plotXMin, plotXMax, plotYMin, plotYMax;
    protected double NoLoadSpeed, StallTorque, NoLoadCurrent, StallCurrent;

    // validation messages for UI
    protected List<string> ErrorMessages { get; set; } = new List<string>();
    protected string? SuccessMessage { get; set; }

    protected EditContext? editContext;
    protected List<string> ValidationErrors { get; set; } = new List<string>();
    protected string? PageAlertMessage { get; set; }
    protected bool PageAlertIsError { get; set; }

    protected void NewGroup()
    {
        SelectedIndex = -1;
        EditingMotor = new MotorModel();
        EditingPoints = new List<MotorDataPoint>();
        editContext = new EditContext(EditingMotor);
    }

    protected async Task SaveGroup()
    {
        if (SelectedIndex >= 0 && SelectedIndex < SavedMotors.Count)
        {
            var id = SavedMotors[SelectedIndex].Id;
            await MotorManager.UpdateMotorAsync(id, EditingMotor, EditingPoints);
        }
        else
        {
            var newId = await MotorManager.AddMotorAsync(EditingMotor, EditingPoints);
            SavedMotors = await MotorManager.GetAllMotorsAsync();
            SelectedIndex = SavedMotors.FindIndex(m => m.Id == newId);
            await LoadSelectedAsync();
            return;
        }

        // reload list to reflect changes
        SavedMotors = await MotorManager.GetAllMotorsAsync();
        // keep current selection where possible
        if (EditingMotor.Id != 0)
        {
            SelectedIndex = SavedMotors.FindIndex(m => m.Id == EditingMotor.Id);
        }
        else
        {
            SelectedIndex = -1;
        }
        await LoadSelectedAsync();
    }

    // Validate point list using DataAnnotations for each point
    protected bool ValidatePoints(out List<string> errors)
    {
        return MotorValidator.ValidatePoints(EditingPoints, out errors);
    }

    protected async Task HandleValidSubmit()
    {
        ValidationErrors.Clear(); PageAlertMessage = null;
        if (!ValidatePoints(out var errs))
        {
            ValidationErrors = errs;
            PageAlertIsError = true;
            PageAlertMessage = "表单验证未通过，请修正下列错误。";
            StateHasChanged();
            return;
        }
        try
        {
            await SaveGroup();
            PageAlertMessage = "保存成功"; PageAlertIsError = false;
            Notifications.Show("保存成功", false);
        }
        catch (Exception ex)
        {
            PageAlertMessage = "保存失败：" + ex.Message;
            PageAlertIsError = true;
            Notifications.Show("保存失败: " + ex.Message, true, 6000);
        }
    }

    protected void HandleInvalidSubmit(EditContext ctx)
    {
        ValidationErrors = ctx.GetValidationMessages().ToList();
        PageAlertIsError = true; PageAlertMessage = "表单验证未通过，请检查输入。";
    }

    protected async Task TriggerFormSubmit()
    {
        if (editContext == null)
        {
            editContext = new EditContext(EditingMotor);
        }
        if (editContext.Validate())
        {
            await HandleValidSubmit();
        }
        else
        {
            HandleInvalidSubmit(editContext);
        }
    }

    protected async Task DeleteGroup()
    {
        if (SelectedIndex >= 0 && SelectedIndex < SavedMotors.Count)
        {
            var id = SavedMotors[SelectedIndex].Id;
            await MotorManager.DeleteMotorAsync(id);
            SavedMotors = await MotorManager.GetAllMotorsAsync();
            SelectedIndex = -1;
            EditingMotor = new MotorModel();
            EditingPoints = new List<MotorDataPoint>();
            StateHasChanged();
        }
    }

    // Load a motor by its Id (used by QuickGrid row action)
    protected async Task LoadMotorById(int id)
    {
        var idx = SavedMotors.FindIndex(m => m.Id == id);
        if (idx >= 0)
        {
            SelectedIndex = idx;
            await LoadSelectedAsync();
        }
    }

    // Delete a motor by its Id (used by QuickGrid row action)
    protected async Task DeleteMotorById(int id)
    {
        try
        {
            await MotorManager.DeleteMotorAsync(id);
        }
        catch (Exception ex)
        {
            PageAlertIsError = true;
            PageAlertMessage = "删除失败: " + ex.Message;
            Notifications.Show(PageAlertMessage, true, 4000);
            StateHasChanged();
            return;
        }
        SavedMotors = await MotorManager.GetAllMotorsAsync();
        // if we deleted the currently selected item, reset editing state
        if (EditingMotor != null && EditingMotor.Id == id)
        {
            SelectedIndex = -1;
            EditingMotor = new MotorModel();
            EditingPoints = new List<MotorDataPoint>();
        }
        PageAlertIsError = false; PageAlertMessage = "删除成功";
        Notifications.Show("删除成功", false);
        StateHasChanged();
    }

    protected void AddPoint()
    {
        EditingPoints.Add(new MotorDataPoint { Torque = 0, Speed = 0, Current = 0 });
    }

    protected void RemovePoint(int idx)
    {
        if (idx >= 0 && idx < EditingPoints.Count)
            EditingPoints.RemoveAt(idx);
    }

    protected static double EvalPoly(double[] coeffs, double x)
    {
        double y = 0;
        for (int i = 0; i < coeffs.Length; i++) y += coeffs[i] * Math.Pow(x, i);
        return y;
    }

    protected void ComputeFits()
    {
        HasFit = false;
        var fit = MotorCalculator.ComputeFits(EditingPoints);
        if (fit == null || fit.Torques == null || fit.Torques.Length < 2) return;

        CurrentCoeffs = fit.CurrentCoeffs;
        SpeedCoeffs = fit.SpeedCoeffs;
        NoLoadSpeed = fit.NoLoadSpeed;
        NoLoadCurrent = fit.NoLoadCurrent;
        StallTorque = fit.StallTorque;
        StallCurrent = fit.StallCurrent;
        plotXMin = fit.PlotXMin; plotXMax = fit.PlotXMax; plotYMin = fit.PlotYMin; plotYMax = fit.PlotYMax;

        HasFit = true;

        var speedTrace = new {
            x = fit.Torques,
            y = fit.Speeds,
            mode = "lines+markers",
            name = "Speed (rpm)",
            line = new { color = "#1f77b4" }
        };
        var currentTrace = new {
            x = fit.Torques,
            y = fit.Currents,
            mode = "lines+markers",
            name = "Current (A)",
            yaxis = "y2",
            line = new { color = "#ff7f0e" }
        };

        var layout = new {
            title = EditingMotor.MotorName ?? "Motor",
            xaxis = new { title = "Torque (Nm)" },
            yaxis = new { title = "Speed (rpm)", side = "left" },
            yaxis2 = new { title = "Current (A)", overlaying = "y", side = "right" },
            legend = new { x = 0.01, y = 0.99 }
        };

        _ = JS.InvokeVoidAsync("plotlyInterop.plot", "plotly-motor-plot", new object[] { speedTrace, currentTrace }, layout);
    }
}
