/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Input, OnChanges, OnDestroy, Renderer2, SimpleChanges, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { Router } from '@angular/router';
import { StatefulControlComponent, Types } from '@app/framework/internal';

export const SQX_IFRAME_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => IFrameEditorComponent), multi: true
};

interface State {
    // True, when the editor is shown as fullscreen.
    isFullscreen: boolean;
}

@Component({
    selector: 'sqx-iframe-editor',
    styleUrls: ['./iframe-editor.component.scss'],
    templateUrl: './iframe-editor.component.html',
    providers: [
        SQX_IFRAME_EDITOR_CONTROL_VALUE_ACCESSOR
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IFrameEditorComponent extends StatefulControlComponent<State, any> implements OnChanges, OnDestroy, AfterViewInit {
    private value: any;
    private isInitialized = false;

    @ViewChild('iframe', { static: false })
    public iframe: ElementRef<HTMLIFrameElement>;

    @ViewChild('container', { static: false })
    public container: ElementRef<HTMLElement>;

    @ViewChild('inner', { static: false })
    public inner: ElementRef<HTMLElement>;

    @Input()
    public context: any = {};

    @Input()
    public formValue: any;

    @Input()
    public language: string;

    @Input()
    public url: string;

    public fullscreen: boolean;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly renderer: Renderer2,
        private readonly router: Router
    ) {
        super(changeDetector, {
            isFullscreen: false
        });
    }

    public ngOnDestroy() {
        this.toggleFullscreen(false);
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (this.iframe) {
            if (changes['url']) {
                this.setupUrl();
            }

            if (changes['formValue']) {
                this.sendFormValue();
            }

            if (changes['language']) {
                this.sendLanguage();
            }
        }
    }

    private setupUrl() {
        this.iframe.nativeElement.src = this.url;
    }

    public ngAfterViewInit() {
        this.setupUrl();

        this.own(
            this.renderer.listen('window', 'message', (event: MessageEvent) => {
                if (event.source === this.iframe.nativeElement.contentWindow) {
                    const { type } = event.data;

                    if (type === 'started') {
                        this.isInitialized = true;

                        this.sendInit();
                        this.sendFullscreen();
                        this.sendFormValue();
                        this.sendLanguage();
                        this.sendDisabled();
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
                    } else if (type === 'valueChanged') {
                        const { value } = event.data;

                        if (!Types.equals(this.value, value)) {
                            this.value = value;

                            this.callChange(value);
                        }
                    } else if (type === 'touched') {
                        this.callTouched();
                    }

                    this.detectChanges();
                }
            }));
    }

    public writeValue(obj: any) {
        this.value = obj;

        this.sendValue();
    }

    public setDisabledState(isDisabled: boolean): void {
        super.setDisabledState(isDisabled);

        this.sendDisabled();
    }

    public reset() {
        this.sendInit();
    }

    private sendInit() {
        this.sendMessage('init', { context: this.context || {} });
    }

    private sendValue() {
        this.sendMessage('valueChanged', { value: this.value });
    }

    private sendFullscreen() {
        this.sendMessage('fullscreenChanged', { fullscreen: this.snapshot.isFullscreen });
    }

    private sendDisabled() {
        this.sendMessage('disabled', { isDisabled: this.snapshot.isDisabled });
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
        if (!this.iframe) {
            return;
        }

        const iframe = this.iframe.nativeElement;

        if (this.isInitialized && iframe.contentWindow && Types.isFunction(iframe.contentWindow.postMessage)) {
            const message = { type, ...payload };

            iframe.contentWindow.postMessage(message, '*');
        }
    }
}