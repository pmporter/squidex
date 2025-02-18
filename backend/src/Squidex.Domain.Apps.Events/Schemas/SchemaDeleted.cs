﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Schemas;

[EventType(nameof(SchemaDeleted))]
public sealed class SchemaDeleted : SchemaEvent
{
    public bool Permanent { get; set; }
}
