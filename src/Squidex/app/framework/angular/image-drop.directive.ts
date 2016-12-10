/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { DragService } from './../services/drag.service';
import { Vec2 } from './../utils/vec2';

@Ng2.Directive({
    selector: '.sqx-image-drop'
})
export class ImageDropDirective {
    constructor(
        private readonly element: Ng2.ElementRef,
        private readonly renderer: Ng2.Renderer,
        private readonly dragService: DragService
    ) {
    }

    @Ng2.HostListener('dragenter', ['$event'])
    public onDragEnter(event: DragDropEvent) {
        this.tryStopEvent(event);
    }

    @Ng2.HostListener('dragover', ['$event'])
    public onDragOver(event: DragDropEvent) {
        this.tryStopEvent(event);
    }

    @Ng2.HostListener('drop', ['$event'])
    public onDrop(event: DragDropEvent) {
        const image = this.findImage(event);

        if (!image) {
            return;
        }

        const position = this.getRelativeCoordinates(event, this.element.nativeElement).round();

        const reader = new FileReader();

        reader.onload = (loadedFile: any) => {
            const imageSource: string = loadedFile.target.result;
            const imageElement = document.createElement('img');

            imageElement.onload = () => {
                this.dragService.emitDrop({
                    position, dropTarget: this.element.nativeElement.id, model: {
                        sizeX: imageElement.width,
                        sizeY: imageElement.height,
                        source: imageSource
                    }
                });
            };
            imageElement.src = imageSource;
        };
        reader.readAsDataURL(image);

        this.stopEvent(event);
    }

    private stopEvent(event: Event) {
        event.preventDefault();
        event.stopPropagation();
    }

    private tryStopEvent(event: DragDropEvent) {
        const hasFiles = this.hasFiles(event.dataTransfer.types);

        if (!hasFiles) {
            return;
        }

        this.stopEvent(event);
    }

    private hasFiles(types: any): boolean {
        if (!types) {
            return false;
        }

        if (isFunction(types.indexOf)) {
            return types.indexOf('Files') !== -1;
        } else if (isFunction(types.contains)) {
            return types.contains('Files');
        } else {
            return false;
        }
    }

    private findImage(event: DragDropEvent): File | null {
        let image: File | null = null;

        /* tslint:disable:prefer-for-of */
        for (let i = 0; i < event.dataTransfer.files.length; i++) {
            const file = event.dataTransfer.files[i];

            if (file.type.match('image.*')) {
                image = file;
                break;
            }
        }

        return image;
    }

    private getRelativeCoordinates(e: any, container: any): Vec2 {
        const rect = container.getBoundingClientRect();

        const pos = { x: 0, y: 0 };

        pos.x = !!e.touches ? e.touches[0].pageX : e.pageX;
        pos.y = !!e.touches ? e.touches[0].pageY : e.pageY;

        return new Vec2(pos.x - rect.left, pos.y - rect.top);
    }
}

function isFunction(obj: any): boolean {
    return !!(obj && obj.constructor && obj.call && obj.apply);
};

interface DragDropEvent extends MouseEvent {
    readonly dataTransfer: DataTransfer;
}