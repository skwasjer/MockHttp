
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "By design: uri's are glob patterns, not matching strict URI format.", Scope = "member", Target = "~M:HttpClientMock.RequestMatchingExtensions.Url(HttpClientMock.RequestMatching,System.String)~HttpClientMock.RequestMatching")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "<Pending>", Scope = "member", Target = "~M:HttpClientMock.HttpRequestMatchers.UrlMatcher.#ctor(System.String)")]