/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, DestroyRef, effect, ElementRef, inject, input, numberAttribute, output, Renderer2, signal, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import { AppLanguageDto, AppsState, AssetDto, AssetSelectorComponent, computeEditorUrl, ContentDto, ContentSelectorComponent, DialogModel, DialogService, disabled$, ModalDirective, SafeResourceUrlPipe, ThemeService, Types, value$ } from '@app/shared';

@Component({
    selector: 'sqx-iframe-editor',
    styleUrls: ['./iframe-editor.component.scss'],
    templateUrl: './iframe-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AssetSelectorComponent,
        ContentSelectorComponent,
        ModalDirective,
        SafeResourceUrlPipe,
    ],
    host: {
        '(window:message)': 'onWindowMessage($event)',
    },
})
export class IFrameEditorComponent {
    private readonly iframe = viewChild<ElementRef<HTMLIFrameElement>>('iframe');
    private readonly container = viewChild.required<ElementRef<HTMLElement>>('container');
    private readonly inner = viewChild.required<ElementRef<HTMLElement>>('inner');
    private readonly isDisabledByControl = signal(false);
    private value: any;
    private isInitialized = false;

    /** Emits when the editor wants to toggle the expanded mode. */
    public readonly isExpandedChange = output();

    /** Renders the editor in expanded mode. */
    public readonly isExpanded = input(false, { transform: booleanAttribute });

    /** The schemas to pick contents from, if the editor does not define them. */
    public readonly schemaIds = input.required<ReadonlyArray<string> | undefined>();

    /** Additional context information that is passed to the editor. */
    public readonly context = input.required<any>();

    /** The value of the whole content form. */
    public readonly formValue = input.required<any>();

    /** The name of the field. */
    public readonly formField = input.required<string>();

    /** The index of the field when it is part of an array. */
    public readonly formIndex = input.required<number | null | undefined, unknown>({ transform: numberAttribute });

    /** The language of the field value. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The form control with the value of the field. */
    public readonly formControlBinding = input.required<AbstractControl>();

    /** Disables the editor, in addition to the disabled state of the form control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** The URL of the editor. */
    public readonly url = input<string | null>();

    public readonly computedUrl = computed(() => {
        return computeEditorUrl(this.url(), this.appsState.snapshot.selectedSettings);
    });

    public readonly isDisabled = computed(() => {
        return this.disabled() || this.isDisabledByControl();
    });

    public readonly isFullscreen = signal(false);

    public assetsCorrelationId: any;
    public readonly assetsDialog = new DialogModel();

    public contentsCorrelationId: any;
    public readonly contentsQuery = signal<string | undefined>(undefined);
    public readonly contentsSchemas = signal<ReadonlyArray<string> | undefined>(undefined);
    public readonly contentsSelectedIds = signal<ReadonlyArray<string> | undefined>(undefined);
    public readonly contentsDialog = new DialogModel();

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly renderer: Renderer2,
        private readonly router: Router,
        private readonly themeService: ThemeService,
    ) {
        const theme = toSignal(themeService.themeChanges, { initialValue: themeService.theme });

        effect(() => {
            this.sendMessage('themeChanged', { theme: theme() });
        });

        effect(() => {
            this.sendFormValue();
        });

        effect(() => {
            this.sendMoved();
        });

        effect(() => {
            this.sendExpanded();
        });

        effect(() => {
            this.sendLanguage();
        });

        effect(() => {
            this.sendContext();
        });

        effect(() => {
            this.sendDisabled();
        });

        effect(onCleanup => {
            const control = this.formControlBinding();
            if (!control) {
                return;
            }

            const subscriptions = [
                value$(control)
                    .subscribe(value => {
                        this.updateValue(value);
                    }),
                disabled$(control)
                    .subscribe(isDisabled => {
                        this.isDisabledByControl.set(isDisabled);
                    }),
            ];

            onCleanup(() => {
                for (const subscription of subscriptions) {
                    subscription.unsubscribe();
                }
            });
        });

        inject(DestroyRef).onDestroy(() => {
            this.toggleFullscreen(false);
        });
    }

    public onWindowMessage(event: MessageEvent) {
        const iframe = this.iframe()?.nativeElement;
        if (!iframe || event.source !== iframe.contentWindow) {
            return;
        }

        const { type } = event.data;

        if (type === 'started') {
            this.isInitialized = true;

            this.sendTheme();
            this.sendInit();
            this.sendFullscreen();
            this.sendExpanded();
            this.sendFormValue();
            this.sendLanguage();
            this.sendDisabled();
            this.sendMoved();
            this.sendValue();
        } else if (type === 'resize') {
            const { height } = event.data;

            this.renderer.setStyle(iframe, 'height', `${height}px`);
        } else if (type === 'navigate') {
            const { url } = event.data;

            this.router.navigateByUrl(url);
        } else if (type === 'fullscreen') {
            const { mode } = event.data;

            if (mode !== this.isFullscreen()) {
                this.toggleFullscreen(mode);
            }
        } else if (type === 'expanded') {
            const { mode } = event.data;

            if (mode !== this.isExpanded()) {
                this.isExpandedChange.emit();
            }
        } else if (type === 'valueChanged') {
            const { value } = event.data;

            if (!Types.equals(this.value, value)) {
                this.value = value;

                this.formControlBinding()?.reset(value);
            }
        } else if (type === 'touched') {
            this.formControlBinding()?.markAsTouched();
        } else if (type === 'notifyInfo') {
            const { text } = event.data;

            if (Types.isString(text)) {
                this.dialogs.notifyInfo(text);
            }
        } else if (type === 'notifyError') {
            const { text } = event.data;

            if (Types.isString(text)) {
                this.dialogs.notifyError(text);
            }
        } else if (type === 'confirm') {
            const { text, title, correlationId } = event.data;

            if (Types.isString(text) && Types.isString(title) && correlationId) {
                this.dialogs.confirm(title, text).subscribe(result => {
                    this.sendMessage('confirmResult', { correlationId, result });
                });
            }
        } else if (type === 'pickAssets') {
            const { correlationId } = event.data;

            if (correlationId) {
                this.assetsCorrelationId = correlationId;
                this.assetsDialog.show();
            }
        } else if (type === 'pickContents') {
            const { correlationId, schemas, query, selectedIds } = event.data;

            if (correlationId) {
                let schemaIds: ReadonlyArray<string> | undefined = undefined;

                if (Types.isArrayOfString(schemas) && schemas.length > 0) {
                    schemaIds = schemas;
                }

                const schemaIdsField = this.schemaIds();
                if (schemaIdsField && schemaIdsField.length > 0) {
                    schemaIds = schemaIdsField;
                }

                this.contentsCorrelationId = correlationId;
                this.contentsQuery.set(query);
                this.contentsSelectedIds.set(Types.isArrayOfString(selectedIds) ? selectedIds : undefined);
                this.contentsSchemas.set(schemaIds);
                this.contentsDialog.show();
            }
        }
    }

    public pickAssets(assets: ReadonlyArray<AssetDto>) {
        if (this.assetsCorrelationId) {
            this.sendMessage('pickAssetsResult', { correlationId: this.assetsCorrelationId, result: assets });

            this.assetsCorrelationId = null;
        }

        this.assetsDialog.hide();
    }

    public pickContents(contents: ReadonlyArray<ContentDto>) {
        if (this.contentsCorrelationId) {
            this.sendMessage('pickContentsResult', { correlationId: this.contentsCorrelationId, result: contents });

            this.contentsCorrelationId = null;
        }

        this.contentsDialog.hide();
    }

    public updateValue(obj: any) {
        if (!Types.equals(obj, this.value)) {
            this.value = obj;

            this.sendValue();
        }
    }

    public reset() {
        this.sendInit();
    }

    private sendInit() {
        this.sendMessage('init', { context: { ...this.context() || {}, field: this.formField() } });
    }

    private sendTheme() {
        this.sendMessage('themeChanged', { theme: this.themeService.theme });
    }

    private sendValue() {
        this.sendMessage('valueChanged', { value: this.value });
    }

    private sendFullscreen() {
        this.sendMessage('fullscreenChanged', { fullscreen: this.isFullscreen() });
    }

    private sendExpanded() {
        this.sendMessage('expandedChanged', { expanded: this.isExpanded() });
    }

    private sendContext() {
        this.sendMessage('contextChanged', { context: this.context() });
    }

    private sendDisabled() {
        this.sendMessage('disabled', { isDisabled: this.isDisabled() });
    }

    private sendFormValue() {
        const formValue = this.formValue();
        if (formValue) {
            this.sendMessage('formValueChanged', { formValue });
        }
    }

    private sendLanguage() {
        const language = this.language();
        if (language) {
            this.sendMessage('languageChanged', { language });
        }
    }

    private sendMoved() {
        const formIndex = this.formIndex();
        if (Types.isNumber(formIndex)) {
            this.sendMessage('moved', { index: formIndex });
        }
    }

    private toggleFullscreen(isFullscreen: boolean) {
        this.isFullscreen.set(isFullscreen);

        let target = this.container().nativeElement;

        if (isFullscreen) {
            target = document.body;
        }

        this.renderer.appendChild(target, this.inner().nativeElement);

        this.sendFullscreen();
    }

    private sendMessage(type: string, payload: any) {
        const iframe = this.iframe()?.nativeElement;
        if (!iframe) {
            return;
        }

        if (this.isInitialized && iframe.contentWindow && Types.isFunction(iframe.contentWindow.postMessage)) {
            const message = { type, ...payload };

            iframe.contentWindow.postMessage(message, '*');
        }
    }
}
