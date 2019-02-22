/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Input, OnChanges, OnInit, Renderer2, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

import { ExternalControlComponent, Types } from '@app/framework/internal';

export const SQX_IFRAME_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => IFrameEditorComponent), multi: true
};

@Component({
    selector: 'sqx-iframe-editor',
    styleUrls: ['./iframe-editor.component.scss'],
    templateUrl: './iframe-editor.component.html',
    providers: [SQX_IFRAME_EDITOR_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IFrameEditorComponent extends ExternalControlComponent<any> implements OnChanges, OnInit {
    private value: any;
    private isDisabled = false;
    private isInitialized = false;

    @ViewChild('iframe')
    public iframe: ElementRef<HTMLIFrameElement>;

    @Input()
    public url: string;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly renderer: Renderer2
    ) {
        super(changeDetector);
    }

    public ngOnChanges() {
        this.iframe.nativeElement.src = this.url;
    }

    public ngOnInit(): void {
        this.own(
            this.renderer.listen('window', 'message', (event: MessageEvent) => {
                if (event.source === this.iframe.nativeElement.contentWindow) {
                    const { type } = event.data;

                    if (type === 'started') {
                        this.isInitialized = true;

                        this.sendMessage({ type: 'disabled', isDisabled: this.isDisabled });
                        this.sendMessage({ type: 'valueChanged', value: this.value });
                    } else if (type === 'resize') {
                        const { height } = event.data;

                        this.iframe.nativeElement.height = height + 'px';
                    } else if (type === 'valueChanged') {
                        const { value } = event.data;

                        if (!Types.jsJsonEquals(this.value, value)) {
                            this.value = value;

                            this.callChange(value);
                        }
                    } else if (type === 'touched') {
                        this.callTouched();
                    }
                }
            }));
    }

    public writeValue(obj: any) {
        this.value = obj;

        this.sendMessage({ type: 'valueChanged', value: this.value });
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        this.sendMessage({ type: 'disabled', isDisabled: this.isDisabled });
    }

    private sendMessage(message: any) {
        const iframe = this.iframe.nativeElement;

        if (this.isInitialized && iframe.contentWindow && Types.isFunction(iframe.contentWindow.postMessage)) {
            iframe.contentWindow.postMessage(message, '*');
        }
    }
}