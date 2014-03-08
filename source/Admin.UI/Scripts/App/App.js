/// <reference path="../Libs/angular.1.2.13.js" />
/// <reference path="../Libs/angular-route.js" />

var app = angular.module("app", ['ngRoute']);
app.config(function ($routeProvider) {
    $routeProvider
        .when("/", {
            controller: 'HomeCtrl',
            templateUrl: 'templates/home.html'
        })
        .when("/list", {
            controller: 'ListUsersCtrl',
            templateUrl: 'templates/users/list.html'
        })
        .when("/create", {
            controller: 'NewUserCtrl',
            templateUrl: 'templates/users/new.html'
        })
        .when("/edit/:id", {
            controller: 'EditUserCtrl',
            templateUrl: 'templates/users/edit.html'
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
        return $http.get("api/users", { params: { filter: filter, start: start, count: count } })
            .then(function (response) {
                return response.data;
            });
    };
    this.createUser = function (username, password) {
        return $http.post("api/users", { username: username, password: password })
            .then(function (response) {
                return response.data;
            },
            function (response) {
                throw (response.data && response.data.message || "Error Creating User");
            });
    };
});

app.controller("LayoutCtrl", function ($scope, admin) {
    $scope.model = {};

    admin.getAdminName().then(function (data) {
        $scope.model.username = data.username;
    });
});

app.controller("HomeCtrl", function ($scope) {
    $scope.model = {};
});

app.controller("ListUsersCtrl", function ($scope, users, $routeParams) {
    $scope.model = {};

    $scope.search = function (filter) {
        $scope.model.users = null;
        $scope.model.waiting = true;

        users.getUsers(filter, 0, 20).then(function (result) {
            $scope.model.waiting = false;
            $scope.model.users = result.users;
        });
    };

    $scope.search($routeParams.filter);
});

app.controller("NewUserCtrl", function ($scope, users) {
    $scope.model = {};

    $scope.create = function (username, password) {
        $scope.model.message = null;
        $scope.model.success = true;

        users.createUser(username, password)
            .then(function () {
                $scope.model.last = username;
                $scope.model.message = "Create Success";
            },
            function (message) {
                $scope.model.success = false;
                $scope.model.message = message;
            });
    };
});

app.controller("EditUserCtrl", function ($scope, users, $routeParams) {
    $scope.model = {
        id: $routeParams.id
    };
});
