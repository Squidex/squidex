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
    private completions: ReadonlyArray<{ name: string, value: string }> = [];

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

    @Input()
    public set completion(value: ReadonlyArray<{ name: string, description: string }> | undefined) {
        if (value) {
            this.completions = value.map(({ name, description }) => ({ value: name, name, meta: 'context', description }));
        } else {
            this.completions = [];
        }
    }

    constructor(changeDetector: ChangeDetectorRef,
        private readonly resourceLoader: ResourceLoaderService
    ) {
        super(changeDetector, {});
    }

    public writeValue(obj: string) {
        if (this.valueMode === 'Json') {
            if (obj === null) {
                this.value = '';
            } else {
                try {
                    this.value = JSON.stringify(obj, undefined, 4);
                } catch (e) {
                    this.value = '';
                }
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
            this.resourceLoader.loadLocalScript('dependencies/ace/ext/modelist.js'),
            this.resourceLoader.loadLocalScript('dependencies/ace/ext/language_tools.js')
        ]).then(() => {
            this.aceEditor = ace.edit(this.editor.nativeElement);

            this.modelist = ace.require('ace/ext/modelist');

            this.aceEditor.setReadOnly(this.snapshot.isDisabled);
            this.aceEditor.setFontSize(14);

            this.setDisabledState(this.snapshot.isDisabled);
            this.setValue(this.value);
            this.setMode();

            const langTools = ace.require('ace/ext/language_tools');

            if (langTools) {
                this.aceEditor.setOptions({
                    enableBasicAutocompletion: true,
                    enableSnippets: true,
                    enableLiveAutocompletion: true
                });

                const previous = this.aceEditor.completers;

                this.aceEditor.completers = [
                    previous[0], {
                        getCompletions: (editor: any, session: any, pos: any, prefix: any, callback: any) => {
                            callback(null, this.completions);
                        },
                        getDocTooltip: (item: any) => {
                            if (item.meta  === 'context' && item.description) {
                                item.docHTML = `<b>${item.value}</b><hr></hr>${item.description}`;
                            }
                        },
                        identifierRegexps: [/[a-zA-Z_0-9\$\-\.\u00A2-\u2000\u2070-\uFFFF]/]
                    }
                ];
            }

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
        let newValueText = this.aceEditor.getValue();
        let newValueOut = newValueText;

        if (this.valueMode === 'Json') {
            const isValid = this.aceEditor.getSession().getAnnotations().length === 0;

            if (isValid) {
                try {
                    newValueOut = JSON.parse(newValueText);
                } catch (e) {
                    newValueOut = null;
                    newValueText = '';
                }
            } else {
                newValueOut = null;
                newValueText = '';
            }
        }

        if (this.value !== newValueText) {
            this.callChange(newValueOut);
        }

        this.value = newValueText;
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