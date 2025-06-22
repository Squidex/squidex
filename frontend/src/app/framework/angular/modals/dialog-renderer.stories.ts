/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, Input } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { ConfirmClickDirective, DialogRendererComponent, DialogService, ErrorDto, LocalizerService, RootViewComponent, TooltipDirective } from '@app/framework';

type TestMode = 'ErrorText' | 'ErrorDetails' | 'Info';

@Component({
    selector: 'sqx-test',
    template: `
        <button class="btn btn-primary" (click)="test()">
            Show {{ mode }}
        </button>
    `,
    imports: [
        DialogRendererComponent,
    ],
})
class TestComponent {
    public readonly dialogs = inject(DialogService);

    @Input()
    public mode: TestMode = 'Info';

    public test() {
        if (this.mode === 'ErrorDetails') {
            const error = new ErrorDto(500,
                'Error in Server',
                'Error Code',
                [
                    'Details 1',
                    'Details 2',
                    'Details 3',
                    'Details 4',
                ],
            );

            this.dialogs.notifyError(error);
        } else if (this.mode === 'ErrorText') {
            this.dialogs.notifyError('Error');
        } else {
            this.dialogs.notifyInfo('Info');
        }
    }
}

export default {
    title: 'Framework/Dialogs',
    component: DialogRendererComponent,
    decorators: [
        moduleMetadata({
            imports: [
                BrowserAnimationsModule,
                ConfirmClickDirective,
                RootViewComponent,
                TestComponent,
                TooltipDirective,
            ],
            providers: [
                DialogService,
                {
                    provide: LocalizerService,
                    useValue: new LocalizerService({
                        'common.no': 'No',
                        'common.remember': 'Remember',
                        'common.yes': 'Yes',
                    }),
                },
            ],
        }),
    ],
    render: args => ({
        props: args,
        template: `
            <sqx-root-view>
                <div class="p-4">
                    <h3>Dialogs</h3>
                    <div class="p-2 d-flex gap-2">
                        <sqx-test [mode]="'ErrorText'" />
                        <sqx-test [mode]="'ErrorDetails'" />
                        <sqx-test [mode]="'Info'" />
                    </div>

                    <h3 class="mt-4">Tooltips</h3>
                    <div class="p-2 d-flex gap-2">
                        <button class="btn btn-secondary" title="Tooltip" titlePosition="top">Top</button>
                        <button class="btn btn-secondary" title="Tooltip" titlePosition="left">Left</button>
                        <button class="btn btn-secondary" title="Tooltip" titlePosition="right">Right</button>
                        <button class="btn btn-secondary" title="Tooltip" titlePosition="bottom">Bottom</button>
                    </div>

                    <h3 class="mt-4">Immediate Tooltips</h3>
                    <div class="p-2 d-flex gap-2">
                        <button class="btn btn-secondary" [titleDelay]="0" title="Tooltip" titlePosition="top">Top</button>
                        <button class="btn btn-secondary" [titleDelay]="0" title="Tooltip" titlePosition="left">Left</button>
                        <button class="btn btn-secondary" [titleDelay]="0" title="Tooltip" titlePosition="right">Right</button>
                        <button class="btn btn-secondary" [titleDelay]="0" title="Tooltip" titlePosition="bottom">Bottom</button>
                    </div>

                    <h3 class="mt-4">Confirm</h3>
                    <div class="p-2 d-flex gap-2">
                        <button
                            class="btn btn-secondary"
                            confirmTitle="Show alert?"
                            confirmText="Really?"
                            (sqxConfirmClick)="alert('Click')">
                            Confirm
                        </button>

                        <button
                            class="btn btn-secondary"
                            confirmTitle="Show alert?"
                            confirmText="Really?"
                            confirmRememberKey="test"
                            (sqxConfirmClick)="alert('Click')">
                            Confirm Remember
                        </button>
                    </div>
                </div>

                <sqx-dialog-renderer />
            </sqx-root-view>
        `,
    }),
} as Meta;

type Story = StoryObj<DialogRendererComponent & { mode: TestMode }>;

export const Primary: Story = {
    args: {
    },
};