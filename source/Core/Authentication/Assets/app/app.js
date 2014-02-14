/// <reference path="../../libs/angular/angular.min.js" />

(function () {
    "use strict";

    console.log("premod");

    var app = angular.module("app", []);

    console.log("postmod");

    //app.filter("alljson", function () {
    //    return function (o) {
    //        return JSON.stringify(o, null, 4);
    //    };
    //});

    app.factory("authentication", function ($q, $http) {
        var providers;
        console.log("authentication factory", providers)
        return {
            signin: function (uid, pwd) {
                var d = $q.defer();
                $http.post("signin", { Username: uid, Password: pwd })
                    .success(function (data) {
                        d.resolve(data);
                    })
                    .error(function (data) {
                        d.reject(data && data.Message || "Unexpected Error");
                    });
                return d.promise;
            },
            signout: function () {
                var d = $q.defer();
                $http.delete("signout")
                    .success(function () {
                        d.resolve();
                    })
                    .error(function (data) {
                        d.reject(data && data.Message || "Unexpected Error");
                    });
                return d.promise;
            },
            getExternalProviders: function () {
                console.log("getExternalProviders", providers)
                var d = $q.defer();
                if (!providers) {
                    $http.get("providers")
                        .success(function (data) {
                            providers = data;
                            d.resolve(data);
                        })
                        .error(function (data) {
                            d.reject();
                        });
                }
                else {
                    if (providers.length) {
                        d.resolve(providers);
                    }
                    else {
                        d.reject();
                    }
                }
                return d.promise;
            }
        };
    });

    app.controller("LoginCtrl", function ($scope, authentication, $location, ReturnUrl) {
        console.log("LoginCtrl");

        $scope.global = {
            user : null
        };

        //$scope.hasErrors = function () {
        //    return appErrors.errors.length;
        //};
        //$scope.errors = appErrors.errors;
        //$scope.clear = function () {
        //    appErrors.clear();
        //};

        $scope.model = {
            success: false,
            error : null,
            providers: null
        };

        //authentication.getExternalProviders().then(function (list) {
        //    $scope.model.providers = list;
        //});

        $scope.login = function () {
            $scope.model.error = null;

            authentication.signin($scope.model.username, $scope.model.password).then(function (user) {
                $scope.model.success = true;
                $scope.global.user = user;
                window.location = ReturnUrl;
                //$location.url(ReturnUrl, true);
            }, function (error) {
                $scope.model.error = error;
            });
        };
    });

    //app.controller("LogoutCtrl", function ($scope, authentication) {
    //    $scope.logout = function () {
    //        $scope.error = null;
    //        authentication.signout().then(function () {
    //            $scope.success = true;
    //            $scope.global.user = null;
    //        }, function (error) {
    //            $scope.error = error;
    //        })
    //    };
    //});

})();
