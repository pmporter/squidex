﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Squidex.Web.Pipeline;

public sealed class AccessTokenQueryMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        if (HasNoAuthHeader(request) && request.Query.TryGetValue("access_token", out var token))
        {
            request.Headers[HeaderNames.Authorization] = $"Bearer {token}";
        }

        return next(context);
    }

    private static bool HasNoAuthHeader(HttpRequest request)
    {
        return string.IsNullOrWhiteSpace(request.Headers[HeaderNames.Authorization]);
    }
}
