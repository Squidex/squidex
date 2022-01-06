/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AssetPathItem } from '@app/shared/internal';

export interface AssetFolderDropdowNode {
    // The child folders.
    children: AssetFolderDropdowNode[];

    // The parent folder.
    parent: AssetFolderDropdowNode | null;

    // True if selected.
    isSelected?: boolean;

    // True if loading
    isLoading?: boolean;

    // True if loaded
    isLoaded?: boolean;

    // True if expanded
    isExpanded?: boolean;

    // The folder.
    item: AssetPathItem;
}
