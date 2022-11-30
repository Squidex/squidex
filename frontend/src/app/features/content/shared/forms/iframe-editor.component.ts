/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, EventEmitter, HostListener, Input, OnChanges, OnDestroy, Output, Renderer2, SimpleChanges, ViewChild } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import { DialogModel, DialogService, disabled$, StatefulComponent, Types, value$ } from '@app/framework';
import { AppLanguageDto, AppsState, AssetDto, computeEditorUrl, ContentDto } from '@app/shared';

interface State {
    // True, when the editor is shown as fullscreen.
    isFullscreen: boolean;
}

@Component({
    selector: 'sqx-iframe-editor[context][formField][formIndex][formValue][formControlBinding][language][languages]',
    styleUrls: ['./iframe-editor.component.scss'],
    templateUrl: './iframe-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IFrameEditorComponent extends StatefulComponent<State> implements OnChanges, OnDestroy {
    private value: any;
    private isInitialized = false;
    private isDisabled = false;

    @ViewChild('iframe', { static: false })
    public iframe!: ElementRef<HTMLIFrameElement>;

    @ViewChild('container', { static: false })
    public container!: ElementRef<HTMLElement>;

    @ViewChild('inner', { static: false })
    public inner!: ElementRef<HTMLElement>;

    @Output()
    public isExpandedChange = new EventEmitter();

    @Input()
    public isExpanded = false;

    @Input()
    public context: any = {};

    @Input()
    public formValue!: any;

    @Input()
    public formField = '';

    @Input()
    public formIndex?: number | null;

    @Input()
    public language!: AppLanguageDto;

    @Input()
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input()
    public formControlBinding!: AbstractControl;

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.updatedisabled(value === true);
    }

    @Input()
    public set url(value: string | undefined | null) {
        this.computedUrl = computeEditorUrl(value, this.appsState.snapshot.selectedSettings);
    }

    public computedUrl = '';

    public assetsCorrelationId: any;
    public assetsDialog = new DialogModel();

    public contentsCorrelationId: any;
    public contentsSchemas?: string[];
    public contentsDialog = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly renderer: Renderer2,
        private readonly router: Router,
    ) {
        super(changeDetector, {
            isFullscreen: false,
        });
    }

    public ngOnDestroy() {
        super.ngOnDestroy();

        this.toggleFullscreen(false);
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['formValue']) {
            this.sendFormValue();
        }

        if (changes['formIndex']) {
            this.sendMoved();
        }

        if (changes['expanded']) {
            this.sendExpanded();
        }

        if (changes['language']) {
            this.sendLanguage();
        }

        if (changes['formControlBinding']) {
            this.unsubscribeAll();

            const control = this.formControlBinding;

            if (control) {
                this.own(value$(control)
                    .subscribe(value => {
                        this.updateValue(value);
                    }));

                this.own(disabled$(control)
                    .subscribe(isDisabled => {
                        this.updatedisabled(isDisabled);
                    }));
            }
        }
    }

    @HostListener('window:message', ['$event'])
    public onWindowMessage(event: MessageEvent) {
        if (event.source === this.iframe.nativeElement.contentWindow) {
            const { type } = event.data;

            if (type === 'started') {
                this.isInitialized = true;

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

                this.renderer.setStyle(this.iframe.nativeElement, 'height', `${height}px`);
            } else if (type === 'navigate') {
                const { url } = event.data;

                this.router.navigateByUrl(url);
            } else if (type === 'fullscreen') {
                const { mode } = event.data;

                if (mode !== this.snapshot.isFullscreen) {
                    this.toggleFullscreen(mode);
                }
            } else if (type === 'expanded') {
                const { mode } = event.data;

                if (mode !== this.isExpanded) {
                    this.isExpandedChange.emit();
                }
            } else if (type === 'valueChanged') {
                const { value } = event.data;

                if (!Types.equals(this.value, value)) {
                    this.value = value;

                    this.formControlBinding?.reset(value);
                }
            } else if (type === 'touched') {
                this.formControlBinding?.markAsTouched();
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
                const { correlationId, schemas } = event.data;

                if (correlationId) {
                    this.contentsCorrelationId = correlationId;
                    this.contentsSchemas = schemas;
                    this.contentsDialog.show();
                }
            }

            this.detectChanges();
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

    public updatedisabled(isDisabled: boolean) {
        if (this.isDisabled !== isDisabled) {
            this.isDisabled = isDisabled;

            this.sendDisabled();
        }
    }

    public reset() {
        this.sendInit();
    }

    private sendInit() {
        this.sendMessage('init', { context: { ...this.context || {}, field: this.formField } });
    }

    private sendValue() {
        this.sendMessage('valueChanged', { value: this.value });
    }

    private sendFullscreen() {
        this.sendMessage('fullscreenChanged', { fullscreen: this.snapshot.isFullscreen });
    }

    private sendExpanded() {
        this.sendMessage('expandedChanged', { expanded: this.isExpanded });
    }

    private sendDisabled() {
        this.sendMessage('disabled', { isDisabled: this.isDisabled });
    }

    private sendFormValue() {
        if (this.formValue) {
            this.sendMessage('formValueChanged', { formValue: this.formValue });
        }
    }

    private sendLanguage() {
        if (this.language) {
            this.sendMessage('languageChanged', { language: this.language });
        }
    }

    private sendMoved() {
        if (Types.isNumber(this.formIndex)) {
            this.sendMessage('moved', { index: this.formIndex });
        }
    }

    private toggleFullscreen(isFullscreen: boolean) {
        this.next({ isFullscreen });

        let target = this.container.nativeElement;

        if (isFullscreen) {
            target = document.body;
        }

        this.renderer.appendChild(target, this.inner.nativeElement);

        this.sendFullscreen();
    }

    private sendMessage(type: string, payload: any) {
        if (!this.iframe?.nativeElement) {
            return;
        }

        const iframe = this.iframe.nativeElement;

        if (this.isInitialized && iframe.contentWindow && Types.isFunction(iframe.contentWindow.postMessage)) {
            const message = { type, ...payload };

            iframe.contentWindow.postMessage(message, '*');
        }
    }
}
