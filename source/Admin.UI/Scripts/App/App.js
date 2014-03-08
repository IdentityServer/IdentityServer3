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

app.service("admin", function ($http) {
    this.getAdminName = function () {
        return $http.get("api/admin").then(function (response) {
            return response.data;
        });
    };
});
app.service("users", function ($http) {
    this.getUsers = function (filter, start, count) {
        return $http.get("api/users", { params: {filter:filter, start:start, count:count} }).then(function (response) {
            return response.data;
        });
    };
});

app.controller("LayoutCtrl", function ($scope, admin) {
    $scope.model = {};

    admin.getAdminName().then(function (data) {
        $scope.model.username = data.username;
    });
});

app.controller("UsersCtrl", function ($scope, users) {
    $scope.model = {};

    $scope.search = function (filter) {
        users.getUsers(filter, 0, 20).then(function (result) {
            $scope.model.users = result.users;
        });
    };

    $scope.search();
});
