using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using YX.Data;
using YX.Models;

namespace YX.Components.Pages
{
    public partial class ThreadData : ComponentBase
    {
        [Inject]
        public MotorDbContext DbContext { get; set; } = default!;
        
        private string searchSize = string.Empty;
        private string searchThread = string.Empty;
        private List<YX.Models.ThreadDataModel> threadData = new List<YX.Models.ThreadDataModel>();
        
        protected override async Task OnInitializedAsync()
        {
            await LoadThreadData();
        }
        
        private async Task LoadThreadData()
        {
            threadData = await DbContext.ThreadData.ToListAsync();
        }
        
        private List<YX.Models.ThreadDataModel> filteredThreadData => threadData.Where(item => 
            (string.IsNullOrEmpty(searchSize) || item.Size.Contains(searchSize)) &&
            (string.IsNullOrEmpty(searchThread) || item.ThreadDesignation.Contains(searchThread))
        ).ToList();
    }
}
