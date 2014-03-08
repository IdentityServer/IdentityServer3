/// <reference path="../Libs/angular.1.2.13.js" />
/// <reference path="../Libs/angular-route.js" />

var app = angular.module("app", ['ngRoute']);
app.config(function ($routeProvider) {
    $routeProvider
        .when("/", {
            controller: 'UsersCtrl',
            templateUrl: 'templates/users.html'
        })
        .otherwise({
            redirectTo:'/'
        });
});

app.service("adminService", function ($http) {
    this.getAdminName = function () {
        return $http.get("api/admin").then(function (response) {
            return response.data;
        });
    };
});

app.controller("LayoutCtrl", function ($scope, adminService) {
    $scope.model = {};

    adminService.getAdminName().then(function (data) {
        $scope.model.username = data.username;
    });
});

app.controller("UsersCtrl", function ($scope) {
});