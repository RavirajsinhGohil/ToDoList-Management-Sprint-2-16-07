$(document).ready(function () {
    let columns = [
        {
            name: "Member Name",
            data: "name",
            title: "Member Name",
            orderable: true,
            searchable: true,
            type: 'num',
        },
        {
            name: "Name",
            data: "reportingPerson",
            title: "Reporting Person",
            orderable: true,
            searchable: true,
            type: 'num',
            render: function (data) {
                if (data.name != null) {
                    return `${data.name}`;
                } else {
                    return '';
                }
            }
        },
        {
            name: "Email",
            data: "email",
            title: "Email",
            orderable: true,
            searchable: true,
            type: 'num',
        },
        {
            name: "Role",
            data: "roleName",
            title: "Role",
            orderable: true,
            searchable: false,
            type: 'num',
        },
        {
            name: "Phone Number",
            data: "phoneNumber",
            title: "Phone Number",
            orderable: false,
            searchable: false,
            type: 'num'
        },
        {
            name: "Projects",
            data: "project",
            title: "Projects",
            orderable: false,
            searchable: false,
            type: 'num',
            render: function (data) {
                if (data.projectName != null) {
                    return `<span>${data.projectName}</span>`;
                } else {
                    return '<span>N/A</span>';
                }
            }
        }
    ]

    if (window.TeamPermissions.canAddEdit || window.TeamPermissions.canDelete) {
        columns.push({
            name: "Actions",
            data: null,
            title: "Actions",
            orderable: false,
            searchable: false,
            render: function (data, type, row) {
                let actionButtons = '';

                if (window.TeamPermissions.canAddEdit) {
                    actionButtons += `
                        <a class="btn btn-info btn-sm text-white" >
                            <i class="fas fa-pencil-alt"></i> Change
                        </a>
                    `;
                }

                if (window.TeamPermissions.canDelete) {
                    actionButtons += `
                        <a class="btn btn-danger btn-sm" onclick="showDeleteEmployeeModal('${row.employeeId}')">
                            UnAssign
                        </a>
                    `;
                }

                return actionButtons || `<span class="text-muted">No Actions</span>`;
            }
        });
    }

    initializeDataTable("#teamMembersTable", "/Team/GetAssignedMembers", columns);



    viewModel = new TeamViewModel();
    ko.applyBindings(viewModel);
    ko.validation.init({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        errorClass: 'text-danger',
        errorMessageClass: 'validationMessage',
        decorateElement: true
    });

    let notAssignedMembersColumns = [
        {
            name: "mainCheckBox",
            data: "userId",
            title: `<input type="checkbox" id="mainCheckBoxHeader" class="form-check-input">`,
            orderable: false,
            searchable: false,
            className: "no-sort",
            type: 'input',
            render: function (data, type, row) {
                return `<input type="checkbox" class="form-check-input row-checkbox" data-id="${data}"/>`;
            }
        },
        {
            name: "Name",
            data: "name",
            title: "Name",
            orderable: true,
            searchable: true,
            type: 'string',
        },
        {
            name: "Email",
            data: "email",
            title: "Email",
            orderable: true,
            searchable: true,
            type: 'string',
        },
        {
            name: "Phone Number",
            data: "phoneNumber",
            title: "Phone Number",
            orderable: true,
            searchable: true,
            type: 'string',
        }
    ]

    initializeDataTable("#notAssignedMembersTable", "/Team/GetNotAssignedMembers", notAssignedMembersColumns);

    $('#addTeamMembersModal').on('shown.bs.modal', function () {
        const $select = $('#notAssignedMembersSelect');

        // Destroy previous select2 if already initialized
        if ($select.hasClass("select2-hidden-accessible")) {
            $select.select2('destroy');
        }

        // Now initialize fresh select2
        $select.select2({
            placeholder: 'Select User(s)',
            width: '100%'
        });
    });
});

