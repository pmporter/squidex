﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;

namespace Squidex.Domain.Users;

public sealed class DefaultUserPictureStore(IAssetStore assetStore) : IUserPictureStore
{
    public Task UploadAsync(string userId, Stream stream,
        CancellationToken ct = default)
    {
        var fileName = GetFileName(userId);

        return assetStore.UploadAsync(fileName, stream, true, ct);
    }

    public Task DownloadAsync(string userId, Stream stream,
        CancellationToken ct = default)
    {
        var fileName = GetFileName(userId);

        return assetStore.DownloadAsync(fileName, stream, default, ct);
    }

    private static string GetFileName(string userId)
    {
        return $"{userId}_0_picture";
    }
}
