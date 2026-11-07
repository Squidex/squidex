/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, booleanAttribute, ChangeDetectionStrategy, Component, computed, DestroyRef, effect, ElementRef, inject, input, model, numberAttribute, signal, untracked, viewChild } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { debounceTime, Subject } from 'rxjs';
import { ResourceLoaderService, ScriptCompletion, ScriptCompletions, ThemeService, Types } from '@app/framework/internal';
import { FocusComponent } from '../forms-helper';

type Editor = AceAjax.Editor & { completers?: AceAjax.Completer[]; completer?: { showDocTooltip: (item: any) => void } };

type Completion = { name: string; value: string };

function getDocHTML(item: AceAjax.Completion) {
    const source = item as unknown as ScriptCompletion;
    if (!source.path || !source.description) {
        return;
    }

    let docHTML = `<b>${item.value}</b><hr></hr>${source.description}`;

    if (source.allowedValues) {
        docHTML += '<div class="mt-3 mb-0">Allowed Values:<ul class="mt-2 mb-2">';

        for (const value of source.allowedValues) {
            docHTML += `<li><code>${value}</code></li>`;
        }

        docHTML += '</ul></div>';
    }

    if (source.deprecationReason) {
        docHTML += `<div class="mt-2 mb-2"><strong>Deprecated</strong>: ${source.deprecationReason}</div>`;
    }

    item.docHTML = docHTML;
}

@Component({
    selector: 'sqx-code-editor',
    styleUrls: ['./code-editor.component.scss'],
    templateUrl: './code-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CodeEditorComponent implements FormValueControl<any>, FocusComponent {
    private readonly destroyRef = inject(DestroyRef);
    private readonly resourceLoader = inject(ResourceLoaderService);
    private readonly themeService = inject(ThemeService);
    private readonly valueChanged = new Subject<boolean>();
    private aceModes?: ModeList;
    private aceTools = false;
    private lastValue: any;
    private valueText = '';

    /** The value of the control. */
    public readonly value = model<any>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** Renders the editor without a border. */
    public readonly borderless = input(false, { transform: booleanAttribute });

    /** The ace mode, for example "ace/mode/javascript". Ignored when a value file is set. */
    public readonly mode = input('ace/mode/javascript');

    /** A file name to derive the ace mode from, for example "file.json". */
    public readonly valueFile = input('');

    /** Handles the value as string, as parsed JSON or as JSON string. */
    public readonly valueMode = input<'String' | 'Json' | 'JsonString'>('String');

    /** Disables autocompletion and snippets. */
    public readonly disableTools = input(false, { transform: booleanAttribute });

    /** The maximum number of lines when the height is "auto". */
    public readonly maxLines = input<number | undefined, unknown>(undefined, { transform: numberAttribute });

    /** The minimum number of lines when the height is "auto". */
    public readonly minLines = input<number | undefined, unknown>(undefined, { transform: numberAttribute });

    /** Renders a single line editor like a text input. */
    public readonly singleLine = input(false, { transform: booleanAttribute });

    /** Enables snippets in autocompletion. */
    public readonly snippets = input(true, { transform: booleanAttribute });

    /** Wraps long lines. */
    public readonly wordWrap = input(false, { transform: booleanAttribute });

    /** The height in pixels, "auto" to grow with the content or "full" to fill the container. */
    public readonly height = input<number | 'auto' | 'full'>('full');

    /** The suggestions for autocompletion. */
    public readonly completion = input<ScriptCompletions | undefined | null>();

    protected readonly editor = viewChild.required<ElementRef<HTMLDivElement>>('editor');

    protected readonly aceEditor = signal<Editor | undefined>(undefined);

    protected readonly completions = computed<ReadonlyArray<Completion>>(() => {
        const completion = this.completion();
        if (!completion) {
            return [];
        }

        return completion.map(source => {
            const { path, description, type, ...other } = source;

            return { value: path, name: path, description, meta: type?.toLowerCase(), path, ...other };
        });
    });

    constructor() {
        const valueSubscription =
            this.valueChanged.pipe(debounceTime(100))
                .subscribe(() => {
                    this.changeValue();
                });

        let isDestroyed = false;

        this.destroyRef.onDestroy(() => {
            isDestroyed = true;
            valueSubscription.unsubscribe();
        });

        afterNextRender(async () => {
            await Promise.all([
                this.resourceLoader.loadLocalScript('dependencies/ace/ace.js'),
                this.resourceLoader.loadLocalScript('dependencies/ace/ext-modelist.js'),
                this.resourceLoader.loadLocalScript('dependencies/ace/ext-language_tools.js'),
            ]);

            if (!isDestroyed) {
                this.initEditor();
            }
        });

        effect(() => {
            const aceEditor = this.aceEditor();
            if (!aceEditor) {
                return;
            }

            const theme = this.themeService.theme();

            aceEditor.setTheme(theme === 'dark' ? 'ace/theme/one_dark' : 'ace/theme/textmate');
        });

        effect(() => {
            const value = this.value();
            const valueMode = this.valueMode();

            untracked(() => {
                // Values that have been set by this component are already shown in the editor.
                if (value === this.lastValue) {
                    return;
                }

                this.lastValue = value;
                this.valueText = formatValue(value, valueMode);
                this.setValue(this.valueText);
            });
        });

        effect(() => {
            this.setMode(this.aceEditor(), this.mode(), this.valueFile());
        });

        effect(() => {
            this.setOptions(this.aceEditor());
        });

        effect(() => {
            this.setWordWrap(this.aceEditor(), this.wordWrap());
        });
    }

    public focus() {
        this.aceEditor()?.focus();
    }

    private initEditor() {
        const aceEditor: Editor = ace.edit(this.editor().nativeElement);
        aceEditor.setFontSize('14px');

        this.aceModes = ace.require('ace/ext/modelist');
        this.aceTools = !!ace.require('ace/ext/language_tools');

        if (this.aceTools && !this.disableTools()) {
            this.setupCompleter(aceEditor);
        }

        aceEditor.on('blur', () => {
            this.changeValue();
            this.touched.set(true);
        });

        aceEditor.on('change', () => {
            this.valueChanged.next(true);
        });

        aceEditor.on('paste', (event: any) => {
            if (this.singleLine()) {
                event.text = event.text.replace(/[\r\n]+/g, ' ');
            }
        });

        this.aceEditor.set(aceEditor);
        this.setValue(this.valueText);
    }

    private setupCompleter(aceEditor: Editor) {
        const originalCompleter = aceEditor.completer!;
        const originalShowDocTooltip = originalCompleter.showDocTooltip;
        originalCompleter.showDocTooltip = item => {
            requestAnimationFrame(() => {
                originalShowDocTooltip.apply(originalCompleter, [item]);
            });
        };

        const completer: AceAjax.Completer = {
            getCompletions: (editor: any, session: any, pos: any, prefix: any, callback: any) => {
                callback(null, this.completions());
            },
            getDocTooltip: getDocHTML,
        };

        const previous = aceEditor.completers || [];
        aceEditor.completers = [...previous, completer];
    }

    private changeValue() {
        const aceEditor = this.aceEditor();
        if (!aceEditor) {
            return;
        }

        let newValueText = aceEditor.getValue();
        let newValueOut: string | null = newValueText;

        if (this.valueMode() === 'Json') {
            const isValid = aceEditor.getSession().getAnnotations().length === 0;

            if (isValid) {
                try {
                    newValueOut = JSON.parse(newValueText);
                } catch {
                    newValueOut = null;
                    newValueText = '';
                }
            } else {
                newValueOut = null;
                newValueText = '';
            }
        }

        if (this.valueText !== newValueText) {
            this.lastValue = newValueOut;
            this.value.set(newValueOut);
        }

        this.valueText = newValueText;
    }

    private setWordWrap(aceEditor: Editor | undefined, wordWrap: boolean) {
        if (!aceEditor || this.singleLine()) {
            return;
        }

        aceEditor.getSession().setUseWrapMode(wordWrap);
    }

    private setMode(aceEditor: Editor | undefined, mode: string, valueFile: string) {
        if (!aceEditor) {
            return;
        }

        if (valueFile && this.aceModes) {
            aceEditor.getSession().setMode(this.aceModes.getModeForPath(valueFile).mode);
        } else {
            aceEditor.getSession().setMode(mode);
        }
    }

    private setOptions(aceEditor: Editor | undefined) {
        if (!aceEditor) {
            return;
        }

        const height = this.height();
        const isDisabled = this.disabled();
        const singleLine = this.singleLine();

        let maxLines = undefined;
        let minLines = undefined;

        if (singleLine) {
            maxLines = 1;
            minLines = 1;
        } else if (Types.isNumber(height)) {
            const lines = height / 15;

            maxLines = lines;
            minLines = lines;
        } else if (height === 'auto') {
            maxLines = this.maxLines() || 500;
            minLines = Math.max(this.minLines() || 0, Math.min(3, maxLines));
        }

        aceEditor.setOptions({
            autoScrollEditorIntoView: singleLine,
            enableBasicAutocompletion: this.aceTools,
            enableLiveAutocompletion: this.aceTools,
            enableSnippets: this.aceTools && !singleLine && this.snippets(),
            highlightActiveLine: !singleLine,
            maxLines,
            minLines,
            printMargin: !singleLine,
            showGutter: !singleLine,
            useWorker: !isDisabled,
        });

        aceEditor.setReadOnly(isDisabled);

        if (isDisabled) {
            aceEditor.getSession().setAnnotations([]);
        }

        aceEditor.commands.bindKey('Enter|Shift-Enter', singleLine ? 'null' : undefined as any);
    }

    private setValue(value: string) {
        const aceEditor = this.aceEditor();
        if (!aceEditor) {
            return;
        }

        aceEditor.setValue(value);
        aceEditor.clearSelection();
    }
}

function formatValue(value: any, valueMode: 'String' | 'Json' | 'JsonString') {
    try {
        if (Types.isNull(value) || Types.isUndefined(value)) {
            return '';
        } else if (Types.isString(value) && valueMode === 'JsonString') {
            return JSON.stringify(JSON.parse(value), undefined, 4);
        } else if (Types.isString(value)) {
            return value;
        } else if (valueMode === 'Json') {
            return JSON.stringify(value, undefined, 4);
        } else {
            return '';
        }
    } catch {
        return '';
    }
}

interface ModeList {
    getModeForPath(path: string): { mode: string };
}
