/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, forwardRef, ElementRef, OnDestroy, ViewChild, Input } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR, FormBuilder } from '@angular/forms';

import { AppComponentBase } from './app.component-base';
import { ApiUrlConfig, ModalView, AppsStoreService, MessageBus, AssetDragged, DialogService, AuthService, Types, ResourceLoaderService } from './../declarations-base';
import { AssetDropHandler } from './asset-drop.handler';

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
export class RichEditorComponent extends AppComponentBase implements ControlValueAccessor, AfterViewInit, OnDestroy {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };
    private tinyEditor: any;
    private tinyInitTimer: any;
    private value: string;
    private isDisabled = false;
    private assetDropHandler: AssetDropHandler;

    public draggedOver = false;
    private assetDraggedSubscription: any;

    @ViewChild('editor')
    public editor: ElementRef;
    @Input()
    public editorOptions: any;

    public assetsDialog = new ModalView();
    public assetsForm = this.formBuilder.group({
        name: ['']
    });

    constructor(dialogs: DialogService, apps: AppsStoreService, authService: AuthService,
        private readonly resourceLoader: ResourceLoaderService,
        private readonly formBuilder: FormBuilder,
        private readonly apiUrlConfig: ApiUrlConfig,
        private readonly messageBus: MessageBus
    ) {
        super(dialogs, apps, authService);
        this.assetDropHandler = new AssetDropHandler(this.apiUrlConfig);

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

    private editorDefaultOptions() {
        const self = this;
        return {
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

                // TODO: expose an observable to which we can subscribe to
                if (Types.isFunction(self.editorOptions.onSetup)) {
                    self.editorOptions.onSetup(editor);
                }

                this.tinyInitTimer =
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
            let editorOptions = { ...self.editorDefaultOptions(), ...self.editorOptions };
            tinymce.init(editorOptions);
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
        let content = this.assetDropHandler.buildDroppedAssetData(event.dragData, event.mouseEvent);
        if (content) {
            this.tinyEditor.execCommand('mceInsertContent', false, content);
        }
    }
}