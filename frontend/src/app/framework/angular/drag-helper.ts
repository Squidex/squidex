/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { Types } from '../utils/types';
import { HTTP } from './http/http-extensions';

export function sorted<T>(event: CdkDragDrop<ReadonlyArray<T>>): T[] {
    const items = <T[]>event.container.data;

    moveItemInArray(items, event.previousIndex, event.currentIndex);

    return items;
}

export function getFiles(files: FileList | ReadonlyArray<HTTP.UploadFile>) {
    if (Types.isArray(files)) {
        return files;
    }

    const result: HTTP.UploadFile[] = [];

    for (let i = 0; i < files.length; i++) {
        result.push(files[i]);
    }

    return result;
}
