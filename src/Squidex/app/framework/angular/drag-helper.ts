/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

export function sorted<T>(event: CdkDragDrop<ReadonlyArray<T>>): ReadonlyArray<T> {
    const items = <T[]>event.container.data;

    moveItemInArray(items, event.previousIndex, event.currentIndex);

    return items;
}