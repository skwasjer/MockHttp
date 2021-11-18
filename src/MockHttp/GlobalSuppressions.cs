﻿// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "By design: URIs can contain wildcards, not matching strict URI format.", Scope = "member", Target = "~M:MockHttp.RequestMatchingExtensions.Url(MockHttp.RequestMatching,System.String)~MockHttp.RequestMatching")]
[assembly: SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "By design: URIs can contain wildcards, not matching strict URI format.", Scope = "member", Target = "~M:MockHttp.RequestMatchingExtensions.RequestUri(MockHttp.RequestMatching,System.String)~MockHttp.RequestMatching")]
[assembly: SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "By design: URIs can contain wildcards, not matching strict URI format.", Scope = "member", Target = "~M:MockHttp.Matchers.UrlMatcher.#ctor(System.String,System.Boolean)")]
[assembly: SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "By design: URIs can contain wildcards, not matching strict URI format.", Scope = "member", Target = "~M:MockHttp.Matchers.RequestUriMatcher.#ctor(System.String,System.Boolean)")]
[assembly: SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Not an URI/URL", Scope = "member", Target = "~M:MockHttp.Matchers.FormDataMatcher.#ctor(System.String)")]
[assembly: SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Not an URI/URL", Scope = "member", Target = "~M:MockHttp.RequestMatchingExtensions.FormData(MockHttp.RequestMatching,System.String)~MockHttp.RequestMatching")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Temp disable, fix globalization later.")]
