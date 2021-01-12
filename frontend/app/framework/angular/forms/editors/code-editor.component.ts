/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { ResourceLoaderService, StatefulControlComponent, Types } from '@app/framework/internal';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { FocusComponent } from './../forms-helper';

declare var ace: any;

export const SQX_CODE_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => CodeEditorComponent), multi: true
};

@Component({
    selector: 'sqx-code-editor',
    styleUrls: ['./code-editor.component.scss'],
    templateUrl: './code-editor.component.html',
    providers: [
        SQX_CODE_EDITOR_CONTROL_VALUE_ACCESSOR
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CodeEditorComponent extends StatefulControlComponent<{}, string> implements AfterViewInit, FocusComponent, OnChanges {
    private aceEditor: any;
    private valueChanged = new Subject();
    private value = '';
    private modelist: any;

    @ViewChild('editor', { static: false })
    public editor: ElementRef;

    @Input()
    public noBorder = false;

    @Input()
    public mode = 'ace/mode/javascript';

    @Input()
    public filePath: string;

    @Input()
    public valueMode: 'String' | 'Json' = 'String';

    @Input()
    public height = 0;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly resourceLoader: ResourceLoaderService
    ) {
        super(changeDetector, {});
    }

    public writeValue(obj: string) {
        if (this.valueMode === 'Json') {
            try {
                this.value = JSON.stringify(obj, undefined, 4);
            } catch (e) {
                this.value = '';
            }
        } else if (Types.isString(obj)) {
            this.value = obj;
        } else {
            this.value = '';
        }

        if (this.aceEditor) {
            this.setValue(this.value);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        super.setDisabledState(isDisabled);

        if (this.aceEditor) {
            this.aceEditor.setReadOnly(isDisabled);
        }
    }

    public focus() {
        if (this.aceEditor) {
            this.aceEditor.focus();
        }
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['filePath'] || changes['mode']) {
            this.setMode();
        }
    }

    public ngAfterViewInit() {
        this.valueChanged.pipe(debounceTime(500))
            .subscribe(() => {
                this.changeValue();
            });

        if (this.height) {
            this.editor.nativeElement.style.height = `${this.height}px`;
        }

        Promise.all([
            this.resourceLoader.loadLocalScript('dependencies/ace/ace.js'),
            this.resourceLoader.loadLocalScript('dependencies/ace/ext/modelist.js')
        ]).then(() => {
            this.aceEditor = ace.edit(this.editor.nativeElement);

            this.modelist = ace.require('ace/ext/modelist');

            this.aceEditor.setReadOnly(this.snapshot.isDisabled);
            this.aceEditor.setFontSize(14);

            this.setDisabledState(this.snapshot.isDisabled);
            this.setValue(this.value);
            this.setMode();

            this.aceEditor.on('blur', () => {
                this.changeValue();
                this.callTouched();
            });

            this.aceEditor.on('change', () => {
                this.valueChanged.next();
            });

            this.detach();
        });
    }

    private changeValue() {
        let newValue = this.aceEditor.getValue();
        let newValueOut = newValue;

        if (this.valueMode === 'Json') {
            const isValid = this.aceEditor.getSession().getAnnotations().length === 0;

            if (isValid) {
                try {
                    newValueOut = JSON.parse(newValue);
                } catch (e) {
                    newValueOut = null;
                    newValue = '';
                }
            } else {
                newValueOut = null;
                newValue = '';
            }
        }

        if (this.value !== newValue) {
            this.callChange(newValueOut);
        }

        this.value = newValue;
    }

    private setMode() {
        if (this.aceEditor) {
            if (this.filePath && this.modelist) {
                const mode = this.modelist.getModeForPath(this.filePath).mode;

                this.aceEditor.getSession().setMode(mode);
            } else {
                this.aceEditor.getSession().setMode(this.mode);
            }
        }
    }

    private setValue(value: string) {
        this.aceEditor.setValue(value);
        this.aceEditor.clearSelection();
    }
}