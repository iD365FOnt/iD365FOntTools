using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.Service;
using Microsoft.Dynamics.AX.Server.Core.Service;
using Microsoft.Dynamics.Framework.Tools.Extensibility;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using System;
using System.Collections.Generic;

namespace iD365FOntTools.Common
{
    /// <summary>
    /// Common utilities for the Add ins
    /// </summary>
    internal static class CommonUtil
    {
        /// <summary>
        /// Get teh DTE object
        /// </summary>
        internal static DTE DTE => CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE;

        /// <summary>
        /// Current VS project
        /// </summary>
        /// <returns></returns>
        internal static VSProjectNode GetCurrentProject()
        {
            Array projects = CommonUtil.DTE.ActiveSolutionProjects as Array;

            if (projects?.Length > 0)
            {
                Project project = projects.GetValue(0) as Project;
                return project?.Object as VSProjectNode;
            }
            return null;
        }

        internal static ProjectItems GetCurrentProjectItems()
        {
            Array projects = CommonUtil.DTE.ActiveSolutionProjects as Array;

            if (projects?.Length > 0)
            {
                Project project = projects.GetValue(0) as Project;
                return project.ProjectItems;
            }
            return null;
        }

        /// <summary>
        /// Current VS Application Context
        /// </summary>
        /// <returns>VSApplicationContext</returns>
        internal static Microsoft.Dynamics.Framework.Tools.Labels.VSApplicationContext GetVSApplicationContext()
        {
            var context = new Microsoft.Dynamics.Framework.Tools.Labels.VSApplicationContext(CommonUtil.DTE.DTE);
            return context;
        }

        /// <summary>
        /// Current model
        /// </summary>
        /// <returns>ModelInfo</returns>
        internal static ModelInfo GetCurrentModel()
        {
            var project = CommonUtil.GetCurrentProject();

            ModelInfo model = project.GetProjectsModelInfo();

            return model;

        }

        /// <summary>
        /// Get model save info, used when creating an element
        /// </summary>
        /// <returns>save info</returns>
        internal static ModelSaveInfo GetCurrentModelSaveInfo()
        {
            var model = CommonUtil.GetCurrentModel();
            // Prepare information needed for saving
            ModelSaveInfo saveInfo = new ModelSaveInfo
            {
                Id = model.Id,
                Layer = model.Layer
            };

            return saveInfo;
        }
        /// <summary>
        /// Add element to the current project
        /// </summary>
        /// <param name="modelElement">Element</param>
        internal static void AddElementToProject(Microsoft.Dynamics.AX.Metadata.Core.MetaModel.INamedObject modelElement)
        {
            var vsProject = CommonUtil.GetCurrentProject();
            if (vsProject != null)
            {
                vsProject.AddModelElementsToProject(new List<MetadataReference>()
                {
                    new MetadataReference(modelElement.Name, modelElement.GetType())
                });
            }
        }

        internal static void DuplicateElementToProject(Microsoft.Dynamics.AX.Metadata.Core.MetaModel.INamedObject modelElement)
        {
            var vsProject = CommonUtil.GetCurrentProject();
            if (vsProject != null)
            {
                vsProject.DuplicateModelElementsInProject(new List<MetadataReference>()
                {
                    new MetadataReference(modelElement.Name, modelElement.GetType())
                });
            }
        }

        /// <summary>
        /// Display any logs to the user
        /// This shows as Modal dialogs, so keep bunch all messages into 1 message if possible
        /// </summary>
        /// <param name="message">message to display in modal form</param>
        internal static void ShowLog(string message)
        {
            CoreUtility.DisplayInfo(message);
        }

        internal static IMetaModelProviders GetMetaModelProviders()
        {
            IMetaModelProviders metaModelProviders = null;
            try
            {
                metaModelProviders = ServiceLocator.GetService<IMetaModelProviders>();
            }
            catch
            {
                metaModelProviders = new ExtensibilityService();
                ServiceLocator.RegisterService<IMetaModelProviders>(metaModelProviders);
            }

            if (metaModelProviders == null)
            {
                metaModelProviders = new ExtensibilityService();
                ServiceLocator.RegisterService<IMetaModelProviders>(metaModelProviders);
            }

            return metaModelProviders;
        }

        internal static IMetaModelService GetModelSaveService()
        {
            // Find current model
            //var modelSaveInfo = Common.CommonUtil.GetCurrentModelSaveInfo();

            // Get the model servivce to save the element in //Create menu item in the right model
            var metaModelService = CommonUtil.GetMetaModelProviders().CurrentMetaModelService;

            return metaModelService;
        }


    }
}
