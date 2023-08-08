/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, forwardRef, Input, numberAttribute, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { debounceTime, Subject } from 'rxjs';
import { ResourceLoaderService, ScriptCompletions, StatefulControlComponent, TypedSimpleChanges, Types } from '@app/framework/internal';
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
export class CodeEditorComponent extends StatefulControlComponent<{}, any> implements AfterViewInit, FocusComponent {
    private aceEditor: any;
    private aceTools: any;
    private valueChanged = new Subject();
    private value = '';
    private modelist: any;
    private completions: ReadonlyArray<{ name: string; value: string }> = [];

    @ViewChild('editor', { static: false })
    public editor!: ElementRef;

    @Input({ transform: booleanAttribute })
    public borderless?: boolean | null;

    @Input()
    public mode = 'ace/mode/javascript';

    @Input()
    public valueFile = '';

    @Input()
    public valueMode: 'String' | 'Json' | 'JsonString' = 'String';

    @Input({ transform: numberAttribute })
    public maxLines: number | undefined;

    @Input({ transform: booleanAttribute })
    public singleLine = false;

    @Input({ transform: booleanAttribute })
    public snippets = true;

    @Input({ transform: booleanAttribute })
    public wordWrap = false;

    @Input({ transform: numberAttribute })
    public height: number | 'auto' | 'full' = 'full';

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @Input()
    public set completion(value: ScriptCompletions | undefined | null) {
        if (value) {
            this.completions = value.map(({ path, description, type, ...other }) => ({ value: path, name: path, description, meta: type?.toLowerCase(), path, ...other }));
        } else {
            this.completions = [];
        }
    }

    constructor(
        private readonly resourceLoader: ResourceLoaderService,
    ) {
        super({});
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.valueFile || changes.mode) {
            this.setMode();
        }

        if (changes.height || changes.maxLines || changes.singleLine || changes.snippets) {
            this.setOptions();
        }

        if (changes.wordWrap) {
            this.setWordWrap();
        }
    }

    public writeValue(obj: any) {
        try {
            if (Types.isNull(obj) || Types.isUndefined(obj)) {
                this.value = '';
            } else if (Types.isString(obj) && this.valueMode === 'JsonString') {
                this.value = JSON.stringify(JSON.parse(obj), undefined, 4);
            } else if (Types.isString(obj)) {
                this.value = obj;
            } else if (this.valueMode === 'Json') {
                this.value = JSON.stringify(obj, undefined, 4);
            } else {
                this.value = '';
            }
        } catch {
            this.value = '';
        }

        if (this.aceEditor) {
            this.setValue(this.value);
        }
    }

    public onDisabled(isDisabled: boolean) {
        if (!this.aceEditor) {
            return;
        }

        this.aceEditor.setReadOnly(isDisabled);
    }

    public focus() {
        if (!this.aceEditor) {
            return;
        }

        this.aceEditor.focus();
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
            this.modelist = ace.require('ace/ext/modelist');

            this.aceEditor = ace.edit(this.editor.nativeElement);
            this.aceEditor.setFontSize(14);
            this.aceTools = ace.require('ace/ext/language_tools');

            this.setValue(this.value);
            this.setMode();
            this.setOptions();
            this.setWordWrap();
            this.onDisabled(this.snapshot.isDisabled);

            if (this.aceTools) {
                const previous = this.aceEditor.completers;

                this.aceEditor.completers = [
                    previous?.[0], {
                        getCompletions: (editor: any, session: any, pos: any, prefix: any, callback: any) => {
                            callback(null, this.completions);
                        },
                        getDocTooltip: (item: any) => {
                            if (item.path && item.description) {
                                item.docHTML = `<b>${item.value}</b><hr></hr>${item.description}`;

                                if (item.allowedValues) {
                                    item.docHTML += '<div class="mt-2 mb-2">Allowed Values:<ul>';

                                    for (const value of item.allowedValues) {
                                        item.docHTML += `<li><code>${value}</code></li>`;
                                    }

                                    item.docHTML += '</ul></div>';
                                }

                                if (item.deprecationReason) {
                                    item.docHTML += `<div class="mt-2 mb-2"><strong>Deprecated</strong>: ${item.deprecationReason}</div>`;
                                }
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

            this.aceEditor.on('paste', (event: any) => {
                if (this.singleLine) {
                    event.text = event.text.replace(/[\r\n]+/g, ' ');
                }
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
        if (!this.aceEditor || this.singleLine) {
            return;
        }

        this.aceEditor.getSession().setUseWrapMode(this.wordWrap);
    }

    private setMode() {
        if (!this.aceEditor) {
            return;
        }

        if (this.valueFile && this.modelist) {
            const mode = this.modelist.getModeForPath(this.valueFile).mode;

            this.aceEditor.getSession().setMode(mode);
        } else {
            this.aceEditor.getSession().setMode(this.mode);
        }
    }

    private setOptions() {
        if (!this.aceEditor) {
            return;
        }

        let maxLines = undefined;
        let minLines = undefined;

        if (!this.singleLine) {
            if (Types.isNumber(this.height)) {
                const lines = this.height / 15;

                maxLines = lines;
                minLines = lines;
            } else if (this.height === 'auto') {
                maxLines = this.maxLines || 500;
                minLines = Math.min(3, maxLines);
            }
        } else {
            maxLines = 1;
            minLines = 1;
        }

        this.aceEditor.setOptions({
            autoScrollEditorIntoView: this.singleLine,
            enableBasicAutocompletion: !!this.aceTools,
            enableLiveAutocompletion: !!this.aceTools,
            enableSnippets: !!this.aceTools && !this.singleLine && this.snippets,
            highlightActiveLine: !this.singleLine,
            maxLines,
            minLines,
            printMargin: !this.singleLine,
            showGutter: !this.singleLine,
        });
    }

    private setValue(value: string) {
        this.aceEditor.setValue(value);
        this.aceEditor.clearSelection();
    }
}
