using System.Text.Json;
using System.Xml.Serialization;
using Core.Entities;
using Core.Enums;
using Domain.Connection;
using Domain.Mapping;
using Host.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models;
using Web.Plugins;

namespace Web.Controllers;

public sealed class SystemsController(ISystemService systemService, SystemDataModelManager systemDataModelManager, ConnectionDefinitionManager connectionDefinitionManager, PluginRegistry pluginRegistry) : Controller
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
        
        ConnectionDefinition? collectorConnectionDefinition = connectionDefinitionManager.LoadCollectorDefinition(systemConfiguration.CollectorName);
        ConnectionDefinition? provisionerConnectionDefinition = null;
        
        if (!string.IsNullOrWhiteSpace(systemConfiguration.ProvisionerName))
        {
            provisionerConnectionDefinition = connectionDefinitionManager.LoadProvisionerDefinition(systemConfiguration.ProvisionerName!);
        }

        List<ConnectionFieldViewModel> collectorConnectionFields = BuildConnectionFields(collectorConnectionDefinition, systemConfiguration.CollectorConnectionConfigurationJson);
        List<ConnectionFieldViewModel> provisionerConnectionFields = BuildConnectionFields(provisionerConnectionDefinition, systemConfiguration.ProvisionerConnectionConfigurationJson);

        SystemDetailsViewModel viewModel = new()
        {
            SystemConfiguration = systemConfiguration,
            IdentityDataModel = identityDataModel,
            CollectorConnectionFields = collectorConnectionFields,
            ProvisionerConnectionFields = provisionerConnectionFields
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
    
    private static List<ConnectionFieldViewModel> BuildConnectionFields(ConnectionDefinition? connectionDefinition, string? connectionValuesJson)
    {
        List<ConnectionFieldViewModel> connectionFields = [];

        if (connectionDefinition == null)
        {
            return connectionFields;
        }

        Dictionary<string, string> connectionValues = DeserializeConnectionValues(connectionValuesJson);

        foreach (ConnectionFieldDefinition fieldDefinition in connectionDefinition.Fields)
        {
            string label = string.IsNullOrWhiteSpace(fieldDefinition.Label) ? fieldDefinition.Name : fieldDefinition.Label;
            string? value;

            if (!connectionValues.TryGetValue(fieldDefinition.Name, out string existingValue))
            {
                value = fieldDefinition.DefaultValue;
            }
            else
            {
                value = existingValue;
            }

            ConnectionFieldViewModel connectionFieldViewModel = new ConnectionFieldViewModel
            {
                Name = fieldDefinition.Name,
                Label = label,
                Description = fieldDefinition.Description,
                FieldType = fieldDefinition.FieldType,
                IsRequired = fieldDefinition.IsRequired,
                IsSecret = fieldDefinition.IsSecret,
                Value = value
            };

            connectionFields.Add(connectionFieldViewModel);
        }

        return connectionFields;
    }

    private static Dictionary<string, string> DeserializeConnectionValues(string? connectionValuesJson)
    {
        Dictionary<string, string> connectionValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(connectionValuesJson))
        {
            return connectionValues;
        }

        try
        {
            Dictionary<string, string>? parsedConnectionValues = JsonSerializer.Deserialize<Dictionary<string, string>>(connectionValuesJson);

            if (parsedConnectionValues == null)
            {
                return connectionValues;
            }

            foreach (KeyValuePair<string, string> connectionValue in parsedConnectionValues)
            {
                connectionValues[connectionValue.Key] = connectionValue.Value;
            }
        }
        catch
        {
            return connectionValues;
        }

        return connectionValues;
    }
}
