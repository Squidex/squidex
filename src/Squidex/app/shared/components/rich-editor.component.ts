/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, forwardRef, ElementRef, OnDestroy, ViewChild, Output, EventEmitter } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR, FormBuilder } from '@angular/forms';

import { MessageBus, AssetDragged, AssetsService, Types, ResourceLoaderService } from './../declarations-base';

declare var tinymce: any;

export const SQX_RICH_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => RichEditorComponent), multi: true
};

@Component({
    selector: 'sqx-rich-editor',
    styleUrls: ['./rich-editor.component.scss'],
    templateUrl: './rich-editor.component.html',
    providers: [SQX_RICH_EDITOR_CONTROL_VALUE_ACCESSOR]
})
export class RichEditorComponent implements ControlValueAccessor, AfterViewInit, OnDestroy {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };
    private tinyEditor: any;
    private tinyInitTimer: any;
    private value: string;
    private isDisabled = false;

    public draggedOver = false;
    private assetDraggedSubscription: any;

    @ViewChild('editor')
    public editor: ElementRef;

    @Output()
    public assetPluginClicked = new EventEmitter<object>();

    public assetsForm = this.formBuilder.group({
        name: ['']
    });

    constructor(private readonly resourceLoader: ResourceLoaderService,
        private readonly formBuilder: FormBuilder,
        private readonly messageBus: MessageBus,
        private readonly assetsService: AssetsService
    ) {
        this.assetDraggedSubscription = this.messageBus.of(AssetDragged).subscribe(message => {
            // only handle images for now
            if (message.assetDto.isImage) {
                if (message.dragEvent === AssetDragged.DRAG_START) {
                    this.draggedOver = true;
                } else {
                    this.draggedOver = false;
                }
            }
        });
    }

    private getEditorOptions() {
        const self = this;
        return {
            toolbar: 'undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | image media assets',
            plugins: 'code,image,media',
            file_picker_types: 'image',
            convert_urls: false,
            setup: (editor: any) => {
                self.tinyEditor = editor;
                self.tinyEditor.setMode(this.isDisabled ? 'readonly' : 'design');

                self.tinyEditor.on('change', () => {
                    const value = editor.getContent();

                    if (this.value !== value) {
                        this.value = value;

                        self.callChange(value);
                    }
                });

                self.tinyEditor.on('blur', () => {
                    self.callTouched();
                });

                editor.addButton('assets', {
                    text: '',
                    icon: 'browse',
                    tooltip: 'Insert Assets',
                    onclick: (event: any) => {
                        self.assetPluginClicked.emit(event);
                    }
                });

                self.tinyInitTimer =
                    setTimeout(() => {
                        self.tinyEditor.setContent(this.value || '');
                    }, 500);
            },
            removed_menuitems: 'newdocument', target: this.editor.nativeElement
        };
    }

    public ngOnDestroy() {
        clearTimeout(this.tinyInitTimer);

        tinymce.remove(this.editor);
        this.assetDraggedSubscription.unsubscribe();
    }

    public ngAfterViewInit() {
        const self = this;
        this.resourceLoader.loadScript('https://cdnjs.cloudflare.com/ajax/libs/tinymce/4.5.4/tinymce.min.js').then(() => {
            tinymce.init(self.getEditorOptions());
        });
    }

    public writeValue(value: string) {
        this.value = Types.isString(value) ? value : '';

        if (this.tinyEditor) {
            this.tinyEditor.setContent(this.value);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (this.tinyEditor) {
            this.tinyEditor.setMode(isDisabled ? 'readonly' : 'design');
        }
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    public onItemDropped(event: any) {
        let content = this.assetsService.buildDroppedAssetData(event.dragData, event.mouseEvent);
        if (content) {
            this.tinyEditor.execCommand('mceInsertContent', false, content);
        }
    }
}