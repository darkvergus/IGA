using Core.Entities;
using Domain.Mapping;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Plugins;

namespace Web.Controllers;

public sealed class SystemsController(ISystemService systemService, SystemDataModelManager systemDataModelManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<SystemConfiguration> systems = await systemService.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return View(systems);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        SystemConfiguration? systemConfiguration = await systemService.GetAsync(id, cancellationToken).ConfigureAwait(false);
        if (systemConfiguration == null)
        {
            return NotFound();
        }

        PluginDataModel? identityDataModel = systemDataModelManager.LoadDataModel(systemConfiguration, "identity");

        SystemDetailsViewModel viewModel = new()
        {
            SystemConfiguration = systemConfiguration,
            IdentityDataModel = identityDataModel
        };

        return View(viewModel);
    }
}