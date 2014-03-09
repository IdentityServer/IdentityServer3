/// <reference path="../Libs/angular.1.2.13.js" />
/// <reference path="../Libs/angular-route.js" />

(function (angular) {
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
            .when("/list/:filter", {
                controller: 'ListUsersCtrl',
                templateUrl: 'templates/users/list.html'
            })
            .when("/list/:filter/:page", {
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
                redirectTo: '/'
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

    app.controller("ListUsersCtrl", function ($scope, users, $sce, $routeParams) {
        $scope.model = {};

        function PagerButton(text, page, enabled, current) {
            this.text = $sce.trustAsHtml(text + "");
            this.page = page;
            this.enabled = enabled;
            this.current = current;
        }

        function Pager(start, count, total, pageSize) {
            this.start = start;
            this.count = count;
            this.total = total;
            this.pageSize = pageSize;

            this.totalPages = Math.ceil(total / pageSize);
            this.currentPage = (start / pageSize) + 1;
            this.canPrev = this.currentPage > 1;
            this.canNext = this.currentPage < this.totalPages;

            this.buttons = [];

            var totalButtons = 7; // ensure this is odd
            var startButton = 1;
            if (this.currentPage > Math.floor(totalButtons/2)) startButton = this.currentPage - Math.floor(totalButtons/2);

            var endButton = startButton + totalButtons - 1;
            if (endButton >= this.totalPages) endButton = this.totalPages;
            if (this.totalPages > totalButtons &&
                (endButton - startButton + 1) < totalButtons) {
                startButton = endButton - totalButtons + 1;
            }

            this.buttons.push(new PagerButton("&laquo;", 1, endButton > totalButtons));

            for (var i = startButton; i <= endButton; i++) {
                this.buttons.push(new PagerButton(i, i, true, i === this.currentPage));
            }

            this.buttons.push(new PagerButton("&raquo;", this.totalPages, endButton < this.totalPages));
        }

        $scope.search = function (filter, page) {
            $scope.model.filter = filter;
            $scope.model.users = null;
            $scope.model.pager = null;
            $scope.model.waiting = true;

            users.getUsers(filter, (page-1)*10, 10).then(function (result) {
                $scope.model.waiting = false;
                $scope.model.users = result.users;
                if (result.users && result.users.length) {
                    $scope.model.pager = new Pager(result.start, result.count, result.total, 10);
                }
            });
        };

        $scope.search($routeParams.filter, $routeParams.page);
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
})(angular);