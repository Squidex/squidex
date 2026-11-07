/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AppsState, AuthService, ChatDialogComponent, DialogModel, ModalDirective, UIOptions } from '@app/shared';

@Component({
    selector: 'sqx-chat-menu',
    styleUrls: ['./chat-menu.component.scss'],
    templateUrl: './chat-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ChatDialogComponent,
        ModalDirective,
    ],
})
export class ChatMenuComponent {
    protected readonly chatDialog = new DialogModel();

    protected readonly hasChatBot = inject(UIOptions).value.canUseChatBot;

    constructor(
        protected readonly appsState: AppsState,
        protected readonly authService: AuthService,
    ) {
    }
}
