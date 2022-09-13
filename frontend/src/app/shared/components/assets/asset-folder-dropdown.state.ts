/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AssetPathItem, LoadingState } from '@app/shared/internal';

export interface AssetFolderDropdowNode extends LoadingState {
    // The child folders.
    children: AssetFolderDropdowNode[];

    // The parent folder.
    parent: AssetFolderDropdowNode | null;

    // True if selected.
    isSelected?: boolean;

    // True if expanded
    isExpanded?: boolean;

    // The folder.
    item: AssetPathItem;
}
