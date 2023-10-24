/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export interface Comment {
    // The timestamp when the comment was created.
    time: string;

    // The actual text.
    text: string;

    // The user token.
    user: string;

    // The url.
    url?: string;

    // Indicates whether this has been read.
    isRead?: boolean;
}