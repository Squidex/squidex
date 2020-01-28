/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, OnChanges, ViewChild } from '@angular/core';

import { ResourceLoaderService } from '@app/shared/internal';

declare var tui: any;

const blackTheme = {
    'common.bi.image': 'https://uicdn.toast.com/toastui/img/tui-image-editor-bi.png',
    'common.bisize.width': '251px',
    'common.bisize.height': '21px',
    'common.backgroundImage': 'none',
    'common.backgroundColor': '#000',
    'common.border': '0px',

    // header
    'header.backgroundImage': 'none',
    'header.backgroundColor': 'transparent',
    'header.border': '0px',

    // load button
    'loadButton.backgroundColor': '#fff',
    'loadButton.border': '1px solid #ddd',
    'loadButton.color': '#222',
    'loadButton.fontFamily': '\'Noto Sans\', sans-serif',
    'loadButton.fontSize': '12px',

    // download button
    'downloadButton.backgroundColor': '#fdba3b',
    'downloadButton.border': '1px solid #fdba3b',
    'downloadButton.color': '#fff',
    'downloadButton.fontFamily': '\'Noto Sans\', sans-serif',
    'downloadButton.fontSize': '12px',

    // main icons
    'menu.normalIcon.path': 'https://unpkg.com/tui-image-editor@3.7.3/dist/svg/icon-d.svg',
    'menu.normalIcon.name': 'icon-d',
    'menu.activeIcon.path': 'https://unpkg.com/tui-image-editor@3.7.3/dist/svg/icon-b.svg',
    'menu.activeIcon.name': 'icon-b',
    'menu.disabledIcon.path': 'https://unpkg.com/tui-image-editor@3.7.3/dist/svg/icon-a.svg',
    'menu.disabledIcon.name': 'icon-a',
    'menu.hoverIcon.path': 'https://unpkg.com/tui-image-editor@3.7.3/dist/svg/icon-c.svg',
    'menu.hoverIcon.name': 'icon-c',
    'menu.iconSize.width': '24px',
    'menu.iconSize.height': '24px',

    // submenu primary color
    'submenu.backgroundColor': '#000',
    'submenu.partition.color': '#3c3c3c',

    // submenu icons
    'submenu.normalIcon.path': 'https://unpkg.com/tui-image-editor@3.7.3/dist/svg/icon-d.svg',
    'submenu.normalIcon.name': 'icon-d',
    'submenu.activeIcon.path': 'https://unpkg.com/tui-image-editor@3.7.3/dist/svg/icon-c.svg',
    'submenu.activeIcon.name': 'icon-c',
    'submenu.iconSize.width': '32px',
    'submenu.iconSize.height': '32px',

    // submenu labels
    'submenu.normalLabel.color': '#8a8a8a',
    'submenu.normalLabel.fontWeight': 'normal',
    'submenu.activeLabel.color': '#fff',
    'submenu.activeLabel.fontWeight': 'normal',

    // checkbox style
    'checkbox.border': '0px',
    'checkbox.backgroundColor': '#fff',

    // range style
    'range.pointer.color': '#fff',
    'range.bar.color': '#666',
    'range.subbar.color': '#d1d1d1',

    'range.disabledPointer.color': '#414141',
    'range.disabledBar.color': '#282828',
    'range.disabledSubbar.color': '#414141',

    'range.value.color': '#fff',
    'range.value.fontWeight': 'normal',
    'range.value.fontSize': '11px',
    'range.value.border': '1px solid #353535',
    'range.value.backgroundColor': '#151515',
    'range.title.color': '#fff',
    'range.title.fontWeight': 'normal',

    // colorpicker style
    'colorpicker.button.border': '1px solid #1e1e1e',
    'colorpicker.title.color': '#fff'
};

@Component({
    selector: 'sqx-image-editor',
    styleUrls: ['./image-editor.component.scss'],
    templateUrl: './image-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ImageEditorComponent implements AfterViewInit, OnChanges {
    private imageEditor: any;
    private isChanged = false;
    private isChangedBefore = false;

    @Input()
    public accessToken: string;

    @Input()
    public imageUrl: string;

    @ViewChild('editor', { static: false })
    public editor: ElementRef;

    constructor(
        private readonly resourceLoader: ResourceLoaderService
    ) {
    }

    public ngOnChanges() {
        if (this.imageEditor && this.imageUrl) {
            this.imageEditor.loadImageFromURL(this.imageUrl);
        }
    }

    public toFile(): Blob | null {
        if (!this.isChanged) {
            return null;
        }

        this.isChanged = false;

        const dataURI = this.imageEditor.toDataURL();

        const byteString = atob(dataURI.split(',')[1]);
        const byteBuffer = new ArrayBuffer(byteString.length);

        const type = dataURI.split(',')[0].split(':')[1].split(';')[0];

        const array = new Uint8Array(byteBuffer);

        for (let i = 0; i < byteString.length; i++) {
            array[i] = byteString.charCodeAt(i);
        }

        return new Blob([array], { type });
    }

    public ngAfterViewInit() {
        const styles = [
            'https://uicdn.toast.com/tui-color-picker/latest/tui-color-picker.css',
            'https://uicdn.toast.com/tui-image-editor/latest/tui-image-editor.css'
        ];

        const scripts = [
            'https://cdnjs.cloudflare.com/ajax/libs/jquery/1.8.3/jquery.min.js',
            'https://cdnjs.cloudflare.com/ajax/libs/fabric.js/3.3.2/fabric.js',
            'https://uicdn.toast.com/tui.code-snippet/latest/tui-code-snippet.min.js',
            'https://uicdn.toast.com/tui-color-picker/latest/tui-color-picker.js',
            'https://uicdn.toast.com/tui-image-editor/latest/tui-image-editor.js'
        ];

        let path = this.imageUrl;

        if (this.accessToken) {
            path += `&access_token=${this.accessToken}`;
        }

        styles.forEach(style => this.resourceLoader.loadStyle(style));
        Promise.all(scripts.map(script => this.resourceLoader.loadScript(script))).then(() => {
            this.imageEditor = new tui.ImageEditor(this.editor.nativeElement, {
                includeUI: {
                    loadImage: {
                        path, name: 'image'
                    },
                    menu: [
                        'crop',
                        'flip',
                        'mask',
                        'filter'
                    ],
                    theme: blackTheme
                },
                cssMaxWidth: 700,
                cssMaxHeight: 500
            });

            this.imageEditor.on('undoStackChanged', () => {
                if (this.isChangedBefore) {
                    this.isChanged = true;
                } else {
                    this.isChangedBefore = true;
                }
            });
        });
    }
}