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
    private readonly uiOptions = inject(UIOptions);

    protected readonly appsState = inject(AppsState);

    protected readonly authService = inject(AuthService);

    protected readonly chatDialog = new DialogModel();

    protected readonly hasChatBot = this.uiOptions.value.canUseChatBot;
}
