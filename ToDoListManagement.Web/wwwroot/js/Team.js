function TeamViewModel() {
    var self = this;

    self.teamMemberData = {
        userId: ko.observable(),
        name: ko.observable(),
        email: ko.observable(),
        roleId: ko.observable(),
        roleName: ko.observable(),
        phoneNumber: ko.observable()
    }

    self.addTeamMembersData = {
        teamMembers: ko.observableArray([]),
        notAssignedMembers: ko.observableArray([]),
        selectedNotAssignedMembers: ko.observableArray([]),
        teamMember: ko.observable()
    }

    self.selectAllMembers = function () {
        var allIds = self.addTeamMembersData.notAssignedMembers().map(x => x.employeeId);
        self.addTeamMembersData.selectedNotAssignedMembers(allIds);

        $('#notAssignedMembersSelect').val(allIds).trigger('change');
    };

    self.openAddTeamMembersModal = function () {
        ajaxCall('/Team/GetNotAssignedMembers', 'GET', null, function (data) {
            self.addTeamMembersData.notAssignedMembers(data);

            // Populate select2 manually
            populateSelect2Options(data);

            // Open modal
            $('#addTeamMembersModal').modal('show');
        });
    };
}
function populateSelect2Options(data) {
    debugger;
    const $select = $('#notAssignedMembersSelect');
    $select.empty();

    data.forEach(member => {
        if (member.employeeId && member.name) {
            const option = new Option(member.name, member.employeeId, false, false);
            $select.append(option);
        }
    });
    $select.select2({
        placeholder: 'Select User(s)',
        width: '100%'
    });
}
