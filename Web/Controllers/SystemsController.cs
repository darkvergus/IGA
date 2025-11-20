using System.Xml.Serialization;
using Core.Entities;
using Core.Enums;
using Domain.Mapping;
using Host.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models;
using Web.Plugins;

namespace Web.Controllers;

public sealed class SystemsController(ISystemService systemService, SystemDataModelManager systemDataModelManager,
    PluginRegistry pluginRegistry) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<SystemConfiguration> systems = await systemService.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return View(systems);
    }

    [HttpGet]
    public IActionResult Create()
    {
        SystemEditViewModel viewModel = new()
        {
            DataSelection = SystemDataType.Identities
        };

        PopulateAvailableConnectors(viewModel);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SystemEditViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            PopulateAvailableConnectors(viewModel);
            return View(viewModel);
        }

        SystemConfiguration systemConfiguration = new()
        {
            CollectorName = viewModel.CollectorName,
            ProvisionerName = viewModel.ProvisionerName,
            DataSelection = viewModel.DataSelection,
            CollectorConnectionConfigurationJson = viewModel.CollectorConnectionConfigurationJson,
            ProvisionerConnectionConfigurationJson = viewModel.ProvisionerConnectionConfigurationJson,
            Name = viewModel.Name
        };

        PluginDataModel? identityDataModel = systemDataModelManager.LoadDataModel(systemConfiguration, "identity");
        if (identityDataModel != null)
        {
            systemConfiguration.IdentityDataModelXml = SerializeDataModel(identityDataModel);
        }

        SystemConfiguration createdSystemConfiguration = await systemService.CreateAsync(systemConfiguration, cancellationToken).ConfigureAwait(false);

        return RedirectToAction(nameof(Details), new { id = createdSystemConfiguration.Id });
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

    private void PopulateAvailableConnectors(SystemEditViewModel viewModel)
    {
        List<SelectListItem> collectorItems = pluginRegistry.GetAllCollectors().Select(collector => new SelectListItem
            {
                Text = collector.Name,
                Value = collector.Name
            }).OrderBy(item => item.Text).ToList();

        List<SelectListItem> provisionerItems = pluginRegistry.GetAllProvisioners().Select(provisioner => new SelectListItem
            {
                Text = provisioner.Name,
                Value = provisioner.Name
            }).OrderBy(item => item.Text).ToList();

        viewModel.AvailableCollectors = collectorItems;
        viewModel.AvailableProvisioners = provisionerItems;
    }

    private static string SerializeDataModel(PluginDataModel dataModel)
    {
        XmlSerializer serializer = new(typeof(PluginDataModel));
        using StringWriter stringWriter = new();
        serializer.Serialize(stringWriter, dataModel);
        return stringWriter.ToString();
    }
}
