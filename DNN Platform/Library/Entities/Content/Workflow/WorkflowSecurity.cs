﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Linq;
using DotNetNuke.Entities.Content.Workflow.Repositories;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.Entities.Content.Workflow
{
    // TODO: add interface metadata documentation
    public class WorkflowSecurity : ServiceLocator<IWorkflowSecurity, WorkflowSecurity>, IWorkflowSecurity
    {
        private const string ReviewPermissionKey = "REVIEW";
        private readonly IUserController _userController = UserController.Instance;
        private readonly IWorkflowManager _workflowManager = WorkflowManager.Instance;
        private readonly IWorkflowStatePermissionsRepository _statePermissionsRepository = WorkflowStatePermissionsRepository.Instance;

        public bool HasStateReviewerPermission(UserInfo user, PortalSettings settings, int stateId)
        {
            var permissions = _statePermissionsRepository.GetWorkflowStatePermissionByState(stateId);

            return user.IsSuperUser ||
                PortalSecurity.IsInRoles(user, settings, settings.AdministratorRoleName) ||
                PortalSecurity.IsInRoles(user, settings, PermissionController.BuildPermissions(permissions.ToList(), ReviewPermissionKey));
        }

        public bool HasStateReviewerPermission(int portalId, int userId, int stateId)
        {
            var user = _userController.GetUserById(portalId, userId);
            var portalSettings = new PortalSettings(portalId);
            return HasStateReviewerPermission(user, portalSettings, stateId);
        }

        public bool HasStateReviewerPermission(int stateId)
        {
            var user = _userController.GetCurrentUserInfo();
            return HasStateReviewerPermission(user, PortalSettings.Current, stateId);
        }

        public bool IsWorkflowReviewer(int workflowId, int userId)
        {
            var workflow = _workflowManager.GetWorkflow(workflowId);
            return workflow.States.Any(contentWorkflowState => HasStateReviewerPermission(workflow.PortalID, userId, contentWorkflowState.StateID));
        }

        public PermissionInfo GetStateReviewPermission()
        {
            return (PermissionInfo)new PermissionController().GetPermissionByCodeAndKey("SYSTEM_CONTENTWORKFLOWSTATE", ReviewPermissionKey)[0];
        }

        protected override Func<IWorkflowSecurity> GetFactory()
        {
            return () => new WorkflowSecurity();
        }
    }
}
