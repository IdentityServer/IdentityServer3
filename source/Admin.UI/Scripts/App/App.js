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
            .when("/list/:filter?/:page?", {
                controller: 'ListUsersCtrl',
                templateUrl: 'templates/users/list.html'
            })
            .when("/create", {
                controller: 'NewUserCtrl',
                templateUrl: 'templates/users/new.html'
            })
            .when("/edit/:subject", {
                controller: 'EditUserCtrl',
                templateUrl: 'templates/users/edit.html'
            })
            .otherwise({
                redirectTo: '/'
            });
    });

    app.service("admin", function ($http, $q) {
        var admin;

        this.getCurrentAdmin = function () {
            if (admin) {
                var def = $q.defer();
                def.resolve(admin);
                return def.promise;
            }
            return $http.get("api/admin").then(function (response) {
                admin = response.data;
                return admin;
            });
        };
    });

    app.service("meta", function ($http) {
        var meta;
        this.getMetadata = function () {
            if (meta) {
                var def = $q.defer();
                def.resolve(meta);
                return def.promise;
            }
            return $http.get("api/meta").then(function (response) {
                meta = response.data;
                return meta;
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
        this.getUser = function (subject) {
            return $http.get("api/users", { params: { subject: subject } })
                .then(function (response) {
                    return response.data;
                },
                function (response) {
                    throw (response.data && response.data.message || "Error Getting User");
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
        this.deleteUser = function (subject) {
            return $http.post("api/users/delete", { subject:subject })
                .then(function (response) {
                },
                function (response) {
                    throw (response.data && response.data.message || "Error Creating User");
                });
        };
        this.setPassword = function (subject, password) {
            return $http.post("api/password", { subject: subject, password: password })
                .then(function () {
                    return;
                }, function (response) {
                    throw (response.data && response.data.message || "Error Setting Password");
                });
        };
        this.addClaim = function (subject, type, value) {
            return $http.post("api/claims/add", { subject: subject, type: type, value: value })
                .then(function () {
                    return;
                }, function (response) {
                    throw (response.data && response.data.message || "Error Adding Claim");
                });
        };
        this.removeClaim = function (subject, type, value) {
            return $http.post("api/claims/remove", { subject: subject, type: type, value: value })
                .then(function () {
                    return;
                }, function (response) {
                    throw (response.data && response.data.message || "Error Removing Claim");
                });
        };
    });

    app.controller("LayoutCtrl", function ($scope, admin) {
        $scope.model = {};

        admin.getCurrentAdmin().then(function (data) {
            $scope.model.username = data.username;
        });
    });

    app.controller("HomeCtrl", function ($scope) {
        $scope.model = {};
    });

    app.controller("ListUsersCtrl", function ($scope, users, $sce, $routeParams, $location) {
        $scope.model = {};

        function PagerButton(text, page, enabled, current) {
            this.text = $sce.trustAsHtml(text + "");
            this.page = page;
            this.enabled = enabled;
            this.current = current;
        }

        function Pager(result, pageSize, filter) {
            this.start = result.start;
            this.count = result.count;
            this.total = result.total;
            this.pageSize = pageSize;
            this.filter = filter;

            this.totalPages = Math.ceil(this.total / pageSize);
            this.currentPage = (this.start / pageSize) + 1;
            this.canPrev = this.currentPage > 1;
            this.canNext = this.currentPage < this.totalPages;

            this.buttons = [];

            var totalButtons = 7; // ensure this is odd
            var pageSkip = 10;
            var startButton = 1;
            if (this.currentPage > Math.floor(totalButtons/2)) startButton = this.currentPage - Math.floor(totalButtons/2);

            var endButton = startButton + totalButtons - 1;
            if (endButton >= this.totalPages) endButton = this.totalPages;
            if (this.totalPages > totalButtons &&
                (endButton - startButton + 1) < totalButtons) {
                startButton = endButton - totalButtons + 1;
            }

            var prevPage = this.currentPage - pageSkip;
            if (prevPage < 1) prevPage = 1;

            var nextPage = this.currentPage + pageSkip;
            if (nextPage > this.totalPages) nextPage = this.totalPages;

            this.buttons.push(new PagerButton("<strong>&lt;&lt;</strong>", 1, endButton > totalButtons));
            this.buttons.push(new PagerButton("<strong>&lt;</strong>", prevPage, endButton > totalButtons));

            for (var i = startButton; i <= endButton; i++) {
                this.buttons.push(new PagerButton(i, i, true, i === this.currentPage));
            }

            this.buttons.push(new PagerButton("<strong>&gt;</strong>", nextPage, endButton < this.totalPages));
            this.buttons.push(new PagerButton("<strong>&gt;&gt;</strong>", this.totalPages, endButton < this.totalPages));
        }

        $scope.search = function (filter) {
            var url = "/list";
            if (filter) {
                url += "/" + filter;
            }
            $location.url(url);
        };

        var filter = $routeParams.filter;
        $scope.model.filter = filter;
        $scope.model.users = null;
        $scope.model.pager = null;
        $scope.model.waiting = true;

        var itemsPerPage = 10;
        var page = $routeParams.page || 1;
        var startItem = (page - 1) * itemsPerPage;

        users.getUsers(filter, startItem, itemsPerPage).then(function (result) {
            $scope.model.waiting = false;
            $scope.model.users = result.users;
            if (result.users && result.users.length) {
                $scope.model.pager = new Pager(result, itemsPerPage, filter);
            }
        });
    });

    app.controller("NewUserCtrl", function ($scope, users, meta) {
        $scope.model = {};

        meta.getMetadata().then(function (result) {
            //$scope.model.claims = [1,2,3];
        });

        $scope.create = function (username, password) {
            $scope.model.message = null;
            $scope.model.success = true;

            users.createUser(username, password)
                .then(function (result) {
                    $scope.model.last = result.subject;
                    $scope.model.message = "Create Success";
                },
                function (message) {
                    $scope.model.success = false;
                    $scope.model.message = message;
                });
        };
    });

    app.controller("EditUserCtrl", function ($scope, users, $routeParams) {
        $scope.model = {};

        function clear() {
            $scope.model.success = false;
            $scope.model.message = null;
        }
        function success(msg) {
            $scope.model.success = true;
            $scope.model.message = msg;
        }
        function error(msg) {
            $scope.model.success = false;
            $scope.model.message = msg;
        }

        function loadUser() {
            users.getUser($routeParams.subject)
                .then(function (result) {
                    $scope.model.user = result;
                }, function (message) {
                    error(message);
                });
        };
        loadUser();

        $scope.setPassword = function (subject, password, confirm) {
            clear();
            if (password === confirm) {
                users.setPassword(subject, password)
                    .then(function () {
                        success("Password Changed");
                    }, function (message) {
                        error(message);
                    });
            }
            else {
                error("Password and Confirmation do not match");
            }
        };

        $scope.addClaim = function (subject, type, value) {
            clear();
            users.addClaim(subject, type, value)
                .then(function () {
                    success("Claim Added");
                    loadUser();
                }, function (message) {
                    error(message);
                });
        };

        $scope.removeClaim = function (subject, type, value) {
            clear();
            users.removeClaim(subject, type, value)
                .then(function () {
                    success("Claim Removed");
                    loadUser();
                }, function (message) {
                    error(message);
                });
        };

        $scope.deleteUser = function (subject) {
            clear();
            users.deleteUser(subject)
                .then(function () {
                    success("User Deleted");
                    $scope.model.user = null;
                }, function (message) {
                    error(message);
                });
        };
    });
})(angular);
