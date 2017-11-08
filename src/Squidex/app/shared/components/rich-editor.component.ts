/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, forwardRef, ElementRef, OnDestroy, ViewChild, Input } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR, FormBuilder } from '@angular/forms';

import { AppComponentBase } from './app.component-base';
import { AssetUrlPipe } from './pipes';
import { ApiUrlConfig, ModalView, AppsStoreService, AssetDto, AssetsService, ImmutableArray, DialogService, AuthService, Pager, Types, ResourceLoaderService } from './../declarations-base';
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
    private assetSelectorClickHandler: any = null;
    public assetsItems: ImmutableArray<AssetDto>;
    public assetsPager = new Pager(0, 0, 12);
    private assetUrlGenerator: AssetUrlPipe;
    private assetsMimeTypes: Array<string> = ['image/png', 'image/jpeg', 'image/jpg', 'image/gif'];
    private assetDropHandler: AssetDropHandler;
    public draggedOver = false;

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
        private readonly assetsService: AssetsService,
        private readonly apiUrlConfig: ApiUrlConfig
    ) {
        super(dialogs, apps, authService);
        this.assetUrlGenerator = new AssetUrlPipe(this.apiUrlConfig);
        this.assetDropHandler = new AssetDropHandler(this.apiUrlConfig);
    }

    private load() {
        this.appNameOnce()
            .switchMap(app => this.assetsService.getAssets(app, this.assetsPager.pageSize, this.assetsPager.skip, undefined, this.assetsMimeTypes))
            .subscribe(dtos => {
                this.assetsItems = ImmutableArray.of(dtos.items);
                this.assetsPager = this.assetsPager.setCount(dtos.total);
            }, error => {
                this.notifyError(error);
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

                self.tinyEditor.on('init', () => {
                    let editorDoc = self.tinyEditor.iframeElement;
                    let dragTarget: any = null;

                    editorDoc.ondragenter = (event: any) => {
                        console.log('dragenter');
                        self.draggedOver = true;
                        dragTarget = event.target;
                    };

                    editorDoc.ondragleave = (event: any) => {
                        if (event.target === dragTarget) {
                            self.draggedOver = false;
                            console.log('dragleave');
                        } else {
                            self.draggedOver = true;
                        }
                    };
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

    public goNext() {
        this.assetsPager = this.assetsPager.goNext();

        this.load();
    }

    public goPrev() {
        this.assetsPager = this.assetsPager.goPrev();

        this.load();
    }

    public ngOnDestroy() {
        clearTimeout(this.tinyInitTimer);

        tinymce.remove(this.editor);
    }

    public ngAfterViewInit() {
        const self = this;

        this.resourceLoader.loadScript('https://cdnjs.cloudflare.com/ajax/libs/tinymce/4.5.4/tinymce.min.js').then(() => {
            let editorOptions = { ...self.editorDefaultOptions(), ...self.editorOptions };
            console.log(editorOptions);
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

    public closeAssetDialog() {
        this.assetsDialog.hide();
        this.assetSelectorClickHandler = null;
    }

    public onAssetClicked(asset: AssetDto) {
        if (this.assetSelectorClickHandler != null) {
            this.assetSelectorClickHandler.cb(this.assetUrlGenerator.transform(asset), {
                description: asset.fileName,
                width: asset.pixelWidth,
                height: asset.pixelHeight
            });
            this.closeAssetDialog();
        }
    }

    public onItemDropped(event: any) {
        this.draggedOver = false;
        let content = this.assetDropHandler.buildDroppedAssetData(event.dragData, event.mouseEvent);
        if (content) {
            this.tinyEditor.execCommand('mceInsertContent', false, content);
        }
    }
}