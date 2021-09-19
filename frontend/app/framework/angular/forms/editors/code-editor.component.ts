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

declare const ace: any;

export const SQX_CODE_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => CodeEditorComponent), multi: true,
};

@Component({
    selector: 'sqx-code-editor',
    styleUrls: ['./code-editor.component.scss'],
    templateUrl: './code-editor.component.html',
    providers: [
        SQX_CODE_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CodeEditorComponent extends StatefulControlComponent<{}, string> implements AfterViewInit, FocusComponent, OnChanges {
    private aceEditor: any;
    private valueChanged = new Subject();
    private value = '';
    private modelist: any;
    private completions: ReadonlyArray<{ name: string; value: string }> = [];

    @ViewChild('editor', { static: false })
    public editor: ElementRef;

    @Input()
    public borderless?: boolean | null;

    @Input()
    public mode = 'ace/mode/javascript';

    @Input()
    public valueFile: string;

    @Input()
    public valueMode: 'String' | 'Json' = 'String';

    @Input()
    public maxLines: number | undefined;

    @Input()
    public wordWrap: boolean;

    @Input()
    public height: number | 'auto' | 'full' = 'full';

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @Input()
    public set completion(value: ReadonlyArray<{ path: string; description: string }> | undefined | null) {
        if (value) {
            this.completions = value.map(({ path, description }) => ({ value: path, name: path, meta: 'context', description }));
        } else {
            this.completions = [];
        }
    }

    constructor(changeDetector: ChangeDetectorRef,
        private readonly resourceLoader: ResourceLoaderService,
    ) {
        super(changeDetector, {});
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['valueFile'] || changes['mode']) {
            this.setMode();
        }

        if (changes['height'] || changes['maxLines']) {
            this.setHeight();
        }

        if (changes['wordWrap']) {
            this.setWordWrap();
        }
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

    public onDisabled(isDisabled: boolean) {
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

        Promise.all([
            this.resourceLoader.loadLocalScript('dependencies/ace/ace.js'),
            this.resourceLoader.loadLocalScript('dependencies/ace/ext/modelist.js'),
            this.resourceLoader.loadLocalScript('dependencies/ace/ext/language_tools.js'),
        ]).then(() => {
            this.aceEditor = ace.edit(this.editor.nativeElement);

            this.modelist = ace.require('ace/ext/modelist');

            this.aceEditor.setReadOnly(this.snapshot.isDisabled);
            this.aceEditor.setFontSize(14);

            this.onDisabled(this.snapshot.isDisabled);

            this.setValue(this.value);
            this.setMode();
            this.setHeight();
            this.setWordWrap();

            const langTools = ace.require('ace/ext/language_tools');

            if (langTools) {
                this.aceEditor.setOptions({
                    enableBasicAutocompletion: true,
                    enableSnippets: true,
                    enableLiveAutocompletion: true,
                });

                const previous = this.aceEditor.completers;

                this.aceEditor.completers = [
                    previous[0], {
                        getCompletions: (editor: any, session: any, pos: any, prefix: any, callback: any) => {
                            callback(null, this.completions);
                        },
                        getDocTooltip: (item: any) => {
                            if (item.meta === 'context' && item.description) {
                                item.docHTML = `<b>${item.value}</b><hr></hr>${item.description}`;
                            }
                        },
                        // eslint-disable-next-line no-useless-escape
                        identifierRegexps: [/[a-zA-Z_0-9\$\-\.\u00A2-\u2000\u2070-\uFFFF]/],
                    },
                ];
            }

            this.aceEditor.on('blur', () => {
                this.changeValue();

                this.callTouched();
            });

            this.aceEditor.on('change', () => {
                this.valueChanged.next(true);
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

    private setWordWrap() {
        if (this.aceEditor) {
            this.aceEditor.getSession().setUseWrapMode(this.wordWrap);
        }
    }

    private setMode() {
        if (this.aceEditor) {
            if (this.valueFile && this.modelist) {
                const mode = this.modelist.getModeForPath(this.valueFile).mode;

                this.aceEditor.getSession().setMode(mode);
            } else {
                this.aceEditor.getSession().setMode(this.mode);
            }
        }
    }

    private setHeight() {
        if (this.aceEditor && this.editor?.nativeElement) {
            if (Types.isNumber(this.height)) {
                const lines = this.height / 15;

                this.aceEditor.setOptions({
                    minLines: lines,
                    maxLines: lines,
                });
            } else if (this.height === 'auto') {
                this.aceEditor.setOptions({
                    minLines: 3,
                    maxLines: this.maxLines || 500,
                });
            }
        }
    }

    private setValue(value: string) {
        this.aceEditor.setValue(value);
        this.aceEditor.clearSelection();
    }
}
