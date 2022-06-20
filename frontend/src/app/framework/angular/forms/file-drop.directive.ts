/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, EventEmitter, HostListener, Input, Output, Renderer2 } from '@angular/core';
import { Types } from '@app/framework/internal';

@Directive({
    selector: '[sqxDropFile]',
})
export class FileDropDirective {
    private dragCounter = 0;

    @Input()
    public allowedFiles?: ReadonlyArray<string>;

    @Input('sqxDropOnlyImages')
    public onlyImages!: boolean;

    @Input('sqxDropNoPaste')
    public noPaste!: boolean;

    @Input('sqxDropDisabled')
    public disabled = false;

    @Output('sqxDropFile')
    public drop = new EventEmitter<ReadonlyArray<File>>();

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    @HostListener('paste', ['$event'])
    public async onPaste(event: ClipboardEvent) {
        if (!this.noPaste) {
            this.stopEvent(event);

            const files = await this.getAllowedFiles(event.clipboardData);

            if (files && !this.disabled) {
                this.drop.emit(files);
            }
        }
    }

    @HostListener('dragend', ['$event'])
    @HostListener('dragleave', ['$event'])
    public onDragEnd(event: DragDropEvent) {
        const hasFile = this.hasAllowedFile(event.dataTransfer);

        if (hasFile) {
            this.dragEnd();
        }
    }

    @HostListener('dragenter', ['$event'])
    public onDragEnter(event: DragDropEvent) {
        const hasFile = this.hasAllowedFile(event.dataTransfer);

        if (hasFile) {
            this.dragStart();
        }
    }

    @HostListener('dragover', ['$event'])
    public onDragOver(event: DragDropEvent) {
        const isFiles = hasFiles(event.dataTransfer);

        if (isFiles) {
            this.stopEvent(event);
        }
    }

    @HostListener('drop', ['$event'])
    public async onDrop(event: DragDropEvent) {
        if (hasFiles(event.dataTransfer)) {
            this.stopDrag(event);

            const files = await this.getAllowedFiles(event.dataTransfer);

            if (files && !this.disabled) {
                this.drop.emit(files);
            }
        }
    }

    private stopEvent(event: Event) {
        event.preventDefault();
        event.stopPropagation();
    }

    private stopDrag(event: DragDropEvent) {
        this.dragEnd(0);
        this.stopEvent(event);
    }

    private dragStart() {
        this.dragCounter++;

        if (this.dragCounter === 1 && !this.disabled) {
            this.renderer.addClass(this.element.nativeElement, 'drag');
        }
    }

    private dragEnd(number?: number) {
        this.dragCounter = number || this.dragCounter - 1;

        if (this.dragCounter === 0 && !this.disabled) {
            this.renderer.removeClass(this.element.nativeElement, 'drag');
        }
    }

    private async getAllowedFiles(dataTransfer: DataTransfer | null) {
        if (!dataTransfer || !hasFiles(dataTransfer)) {
            return null;
        }

        const files: File[] = [];

        const items = getItems(dataTransfer);

        // Loop over files first, otherwise Chromes deletes them in the async call.
        for (const item of items) {
            const file = item.getAsFile();

            if (file && this.isAllowedFile(file)) {
                files.push(file);
            }
        }

        for (const item of items) {
            if (Types.isFunction(item['webkitGetAsEntry'])) {
                const webkitEntry = item.webkitGetAsEntry();

                if (webkitEntry && webkitEntry.isDirectory) {
                    // eslint-disable-next-line no-await-in-loop
                    await this.transferWebkitTree(webkitEntry, files);
                }
            }
        }

        if (files.length === 0) {
            return null;
        }

        return files;
    }

    private async transferWebkitTree(item: any, files: File[]) {
        if (item.isFile) {
            const file = await getFilePromise(item);

            if (file && this.isAllowedFile(file)) {
                files.push(file);
            }
        } else if (item.isDirectory) {
            const entries = await getFilesPromise(item);

            for (const entry of entries) {
                // eslint-disable-next-line no-await-in-loop
                await this.transferWebkitTree(entry, files);
            }
        }
    }

    private hasAllowedFile(dataTransfer: DataTransfer | null) {
        if (!dataTransfer || !hasFiles(dataTransfer)) {
            return null;
        }

        for (const file of getFiles(dataTransfer)) {
            if (this.isAllowedFile(file)) {
                return true;
            }
        }

        for (const item of getItems(dataTransfer)) {
            if (this.isAllowedFile(item)) {
                return true;
            }
        }

        return false;
    }

    private isAllowedFile(file: { type: string }) {
        return this.isAllowed(file) && this.isImage(file);
    }

    private isAllowed(file: { type: string }) {
        return !this.allowedFiles || this.allowedFiles.includes(file.type);
    }

    private isImage(file: { type: string }) {
        return !this.onlyImages || file.type.indexOf('image/') === 0;
    }
}

function hasFiles(dataTransfer: DataTransfer): boolean {
    if (!dataTransfer || !dataTransfer.types) {
        return false;
    }

    const types: any = dataTransfer.types;

    if (Types.isFunction(types.indexOf)) {
        return types.indexOf('Files') !== -1;
    } else if (Types.isFunction(types.contains)) {
        return types.contains('Files');
    } else {
        return false;
    }
}

function getFilesPromise(item: any): Promise<ReadonlyArray<any>> {
    return new Promise((resolve, reject) => {
        try {
            const reader = item.createReader();

            reader.readEntries(resolve);
        } catch (ex) {
            reject(ex);
        }
    });
}

function getFilePromise(item: any): Promise<File> {
    return new Promise((resolve, reject) => {
        try {
            item.file(resolve);
        } catch (ex) {
            reject(ex);
        }
    });
}

function getItems(dataTransfer: DataTransfer) {
    const result: DataTransferItem[] = [];

    if (dataTransfer.files) {
        for (let i = 0; i < dataTransfer.items.length; i++) {
            const item = dataTransfer.items[i];

            if (item) {
                result.push(item);
            }
        }
    }

    return result;
}

function getFiles(dataTransfer: DataTransfer) {
    const result: File[] = [];

    if (dataTransfer.files) {
        for (let i = 0; i < dataTransfer.files.length; i++) {
            const file = dataTransfer.files[i];

            if (file) {
                result.push(file);
            }
        }
    }

    return result;
}

interface DragDropEvent extends MouseEvent {
    readonly dataTransfer: DataTransfer;
}
