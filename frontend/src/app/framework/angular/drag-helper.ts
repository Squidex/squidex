/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { Types } from './../utils/types';

export function sorted<T>(event: CdkDragDrop<ReadonlyArray<T>>): ReadonlyArray<T> {
    const items = <T[]>event.container.data;

    moveItemInArray(items, event.previousIndex, event.currentIndex);

    return items;
}

export function getFiles(files: FileList | ReadonlyArray<File>) {
    if (Types.isArray(files)) {
        return files;
    }

    const result: File[] = [];

    for (let i = 0; i < files.length; i++) {
        result.push(files[i]);
    }

    return result;
}
