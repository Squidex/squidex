/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, forwardRef, ElementRef, EventEmitter, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR, FormBuilder } from '@angular/forms';

import {
    AssetDto,
    AssetDragged,
    MessageBus,
    ResourceLoaderService,
    Types
} from './../declarations-base';

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
export class RichEditorComponent implements ControlValueAccessor, AfterViewInit, OnInit, OnDestroy {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };
    private tinyEditor: any;
    private tinyInitTimer: any;
    private value: string;
    private isDisabled = false;
    private assetDraggedSubscription: any;

    @ViewChild('editor')
    public editor: ElementRef;

    @Output()
    public assetPluginClicked = new EventEmitter<any>();

    public draggedOver = false;

    public assetsForm = this.formBuilder.group({
        name: ['']
    });

    constructor(private readonly resourceLoader: ResourceLoaderService,
        private readonly formBuilder: FormBuilder,
        private readonly messageBus: MessageBus
    ) {
    }

    public ngOnDestroy() {
        clearTimeout(this.tinyInitTimer);

        tinymce.remove(this.editor);

        this.assetDraggedSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.assetDraggedSubscription =
            this.messageBus.of(AssetDragged).subscribe(message => {
                if (message.assetDto.isImage) {
                    if (message.dragEvent === AssetDragged.DRAG_START) {
                        this.draggedOver = true;
                    } else {
                        this.draggedOver = false;
                    }
                }
            });
    }

    public ngAfterViewInit() {
        const self = this;

        this.resourceLoader.loadScript('https://cdnjs.cloudflare.com/ajax/libs/tinymce/4.5.4/tinymce.min.js').then(() => {
            tinymce.init(self.getEditorOptions());
        });
    }

    private getEditorOptions() {
        const self = this;

        return {
            convert_fonts_to_spans: true,
            convert_urls: false,
            plugins: 'code image media link',
            removed_menuitems: 'newdocument',
            resize: true,
            theme: 'modern',
            toolbar: 'undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image media | assets',
            setup: (editor: any) => {
                self.tinyEditor = editor;
                self.tinyEditor.setMode(this.isDisabled ? 'readonly' : 'design');

                self.tinyEditor.addButton('assets', {
                    text: '',
                    icon: 'assets',
                    tooltip: 'Insert Assets',
                    onclick: (event: any) => {
                        self.assetPluginClicked.emit();
                    }
                });

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

                self.tinyInitTimer =
                    setTimeout(() => {
                        self.tinyEditor.setContent(this.value || '');
                    }, 500);
            },

            target: this.editor.nativeElement
        };
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
        const content = event.dragData;

        if (content instanceof AssetDto) {
            const img = `<img src="${content.url}" alt="${content.fileName}" />`;

            this.tinyEditor.execCommand('mceInsertContent', false, img);
        }

        this.draggedOver = false;
    }
}