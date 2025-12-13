目的：快速让 AI 编码代理在此仓库中立即高效工作。

**Repository Summary**
- **Type:** Blazor Interactive Server Components app (Razor components + interactive server render).
- **Entry:** `Program.cs` — 注册了 `AddRazorComponents()`、`AddInteractiveServerComponents()`；路由由 `Components/Routes.razor` 管理。
- **Target:** `net10.0` (见 `YX.csproj`)；需要 .NET 10 SDK 安装以构建/运行。

**How To Build & Run (dev)**
- Build: `dotnet build YX.sln` 或 `cd YX && dotnet build`。
- Run (local): `cd YX && dotnet run` 或使用 `dotnet run --project YX/YX.csproj`。
- Hot reload: 可尝试 `dotnet watch run --project YX`（基于 SDK 支持）。
- Output artefacts: `bin/Debug/net10.0/`（包含静态 web assets 与生成的 scoped css）。

**Key Files & Structure**
- `Program.cs` — 中心启动与中间件：`UseAntiforgery()`、`UseStatusCodePagesWithReExecute("/not-found")`、`MapRazorComponents<App>().AddInteractiveServerRenderMode()`。
- `YX.csproj` — 目标框架与依赖（示例：`MathNet.Numerics`, `Plotly.Blazor`）。
- `Components/` — 所有 UI 代码：`Pages/`（页面）、`Layout/`（布局）、`Routes.razor`（Router）、`NavMenu.razor`（侧栏导航）。
- `wwwroot/` — 静态资源（CSS/JS/第三方库）。

**Project Conventions & Patterns (discoverable rules)**
- 页面以 Razor 组件形式存在于 `Components/Pages`，并以 `@page "/path"` 声明路由。例如 `Components/Pages/Home.razor` 使用 `@page "/"`。
- 路由发现由 `Components/Routes.razor` 的 `Router AppAssembly="typeof(Program).Assembly"` 完成 —— 新增页面只要位于同一程序集即可被自动发现。
- 布局统一放在 `Components/Layout/MainLayout.razor`，`@Body` 用于插入页面内容。
- 导航使用 `NavLink`（相对路径，例如 `href="counter"`），不要硬编码 `/` 前缀以保持一致性（参见 `NavMenu.razor`）。
- 不要直接修改 `obj/scopedcss` 下的生成文件；修改对应的 `.razor.css` 或组件 CSS 源文件。
- `YX.csproj` 包含 `BlazorDisableThrowNavigationException` 设置：项目已配置为在导航异常上使用特定行为。

**Integration Points & Dependencies**
- Plotly charts and numerical computations: `Plotly.Blazor`, `MathNet.Numerics` — 如果添加可视化或运算模块，优先在 `YX.csproj` 中添加依赖并在组件中按需注入或引用。
- 静态资源/JS 交互：放置到 `wwwroot/` 并在组件中通过 JS interop 使用。

**Adding a New Page (example)**
1. 在 `Components/Pages` 新建 `MyPage.razor`：

```
@page "/mypage"
<h3>My Page</h3>
```

2. 在 `Components/Layout/NavMenu.razor` 添加对应 `NavLink`：

```
<NavLink class="nav-link" href="mypage">My Page</NavLink>
```

因为 `Router` 使用 `typeof(Program).Assembly`，保存后页面会被发现并可访问。

**Debug / Error handling notes**
- `Program.cs` 将生产环境错误处理路由到 `/Error`，并将未找到页面重定向到 `/not-found`。调试时可直接访问这些路径以复现错误页面行为。
- 项目启用了 `UseAntiforgery()`：若新增传统表单端点，确保正确处理 antiforgery token。

**Tests / CI**
- 当前仓库内未发现测试项目或 CI 配置；自动化/测试框架需要额外添加（例如 `dotnet new xunit` 并加入到解决方案）。

**When To Ask A Human**
- 如果需要访问外部服务凭据、注册第三方 API keys、或修改生产 HSTS/HTTPS 配置，请请求有权限的开发者。

如果有特定偏好（例如代码样式、是否添加单元测试、CI 平台），请告诉我，我会把这些偏好合并到说明中。是否需要我把本说明同步到仓库 PR？
