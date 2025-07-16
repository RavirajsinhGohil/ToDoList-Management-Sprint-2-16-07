function TeamViewModel() {
    var self = this;
    
    self.teamMemberData = {
        userId : ko.observable(),
        name: ko.observable(),
        email: ko.observable(),
        roleId: ko.observable(),
        roleName: ko.observable(),
        phoneNumber: ko.observable()
    }

    self.addTeamMembersData = {
        notAssignedMembers: ko.observableArray([]),
        teamMembers: ko.observableArray([]),
        teamMember: ko.observable()
    }

    self.openAddTeamMembersModal = function () {
        ajaxCall('/Team/GetTeamMembersJson', 'GET', null, function (data) {
            self.addTeamMembersData.teamMembers(data);
        });
        $("#addTeamMembersModal").modal('show');
    };
}
