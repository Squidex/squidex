/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Input, ViewChild } from '@angular/core';
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
export class CodeEditorComponent extends StatefulControlComponent<undefined, string> implements AfterViewInit, FocusComponent {
    private valueChanged = new Subject();
    private aceEditor: any;
    private value: string;
    private isDisabled = false;

    @ViewChild('editor', { static: false })
    public editor: ElementRef;

    @Input()
    public noBorder = false;

    @Input()
    public mode = 'ace/mode/javascript';

    @Input()
    public height = 0;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly resourceLoader: ResourceLoaderService
    ) {
        super(changeDetector, undefined);
    }

    public writeValue(obj: string) {
        this.value = Types.isString(obj) ? obj : '';

        if (this.aceEditor) {
            this.setValue(this.value);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (this.aceEditor) {
            this.aceEditor.setReadOnly(isDisabled);
        }
    }

    public focus() {
        if (this.aceEditor) {
            this.aceEditor.focus();
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

        this.resourceLoader.loadLocalScript('dependencies/ace/ace.js').then(() => {
            this.aceEditor = ace.edit(this.editor.nativeElement);

            this.aceEditor.getSession().setMode(this.mode);
            this.aceEditor.setReadOnly(this.isDisabled);
            this.aceEditor.setFontSize(14);

            this.setValue(this.value);

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
        const newValue = this.aceEditor.getValue();

        if (this.value !== newValue) {
            this.callChange(newValue);
        }

        this.value = newValue;
    }

    private setValue(value: string) {
        this.aceEditor.setValue(value || '');
        this.aceEditor.clearSelection();
    }
}