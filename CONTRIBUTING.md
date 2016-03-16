# How to contribute

The easiest ways to contribute is to open an issue and start a discussion. 
Then we can decided if and how a feature or a change could be implemented and if you should submit a pull requests with code changes.

## General questions around token-based authentication and architecture?
We monitor the `identityserver3` tag on [StackOverflow](https://stackoverflow.com/questions/tagged/?tagnames=identityserver3&sort=newest).

## Feedback and issues?
Please start a discussion on the [core repo issue tracker](https://github.com/IdentityServer/IdentityServer3/issues).

## Bugs and feature requests?
Please log a new issue in the appropriate GitHub repo:

* [Core](https://github.com/IdentityServer/IdentityServer3)
* [WS-Federation plugin](https://github.com/IdentityServer/IdentityServer3.WsFederation)
* [EntityFramework support](https://github.com/IdentityServer/IdentityServer3.EntityFramework)
* [ASP. NET Identity support](https://github.com/IdentityServer/IdentityServer3.AspNetIdentity)
* [MembershipReboot support](https://github.com/IdentityServer/IdentityServer3.MembershipReboot)
* [AccessToken validation](https://github.com/IdentityServer/IdentityServer3.AccessTokenValidation)
* [Samples](https://github.com/IdentityServer/IdentityServer3.Samples)

Due to the security nature of IdentityServer, errors are very vague. Make sure you read and understand how to enable [logging](https://identityserver.github.io/Documentation/docsv2/configuration/logging.html) before opening an issue/bug report.

## Chat with us and other identityserver users
https://gitter.im/IdentityServer/IdentityServer3

## Filing issues
The best way to get your bug fixed is to be as detailed as you can be about the problem.
Providing a minimal project with steps to reproduce the problem is ideal.
Here are questions you can answer before you file a bug to make sure you're not missing any important information.

1. Did you read the [documentation](https://identityserver.github.io/Documentation)?
2. Did you include the snippet of broken code in the issue?
3. What are the *EXACT* steps to reproduce this problem?

GitHub supports [markdown](http://github.github.com/github-flavored-markdown/), so when filing bugs make sure you check the formatting before clicking submit.

## Contributing code and content
You will need to sign a [Contributor License Agreement](https://cla.dotnetfoundation.org/) before submitting your pull request. 

Make sure you can build the code. Familiarize yourself with the project workflow and our coding conventions. If you don't know what a pull request is read this article: https://help.github.com/articles/using-pull-requests.

**We only accept PRs to the dev branch.**

Before submitting a feature or substantial code contribution please discuss it with the team and ensure it follows the product roadmap. Here's a list of blog posts that are worth reading before doing a pull request:

* [Open Source Contribution Etiquette](http://tirania.org/blog/archive/2010/Dec-31.html) by Miguel de Icaza
* [Don't "Push" Your Pull Requests](http://www.igvita.com/2011/12/19/dont-push-your-pull-requests/) by Ilya Grigorik.
* [10 tips for better Pull Requests](http://blog.ploeh.dk/2015/01/15/10-tips-for-better-pull-requests/) by Mark Seemann
* [How to write the perfect pull request](https://github.com/blog/1943-how-to-write-the-perfect-pull-request) by GitHub

Here's a few things you should always do when making changes to the code base:

**Commit/Pull Request Format**

```
Summary of the changes (Less than 80 chars)
 - Detail 1
 - Detail 2

#bugnumber (in this specific format)
```

**Tests**

-  Tests need to be provided for every bug/feature that is completed.
-  Tests only need to be present for issues that need to be verified by QA (e.g. not tasks)
-  If there is a scenario that is far too hard to test there does not need to be a test for it.
  - "Too hard" is determined by the team as a whole.

**Contribution Packages**

If you are planning to add functionality via contribution packages, we would like you to use `IdentityServer3.Contrib.*` as a naming convention.
