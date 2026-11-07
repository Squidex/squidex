/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable, ReplaySubject, Subject } from 'rxjs';
import { HTTP, MathHelper, ModalDialogComponent, TooltipDirective, TranslatePipe } from '@app/framework';
import { AppsState, AuthService, ChatEventDto, TranslationsService } from '@app/shared/internal';
import { ChatItemComponent } from './chat-item.component';

type ChatItem = { content: string | Observable<ChatEventDto>; type: 'User' | 'Assistant' | 'System' };

@Component({
    selector: 'sqx-chat-dialog',
    styleUrls: ['./chat-dialog.component.scss'],
    templateUrl: './chat-dialog.component.html',
    imports: [
        ChatItemComponent,
        FormsModule,
        ModalDialogComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class ChatDialogComponent {
    private readonly appsState = inject(AppsState);
    private readonly authService = inject(AuthService);
    private readonly translator = inject(TranslationsService);

    /** Emits the content the user wants to use, or nothing when the dialog is closed. */
    public readonly contentSelect = output<string | HTTP.UploadFile | undefined | null>();

    /** The name of the chat configuration on the server. */
    public readonly configuration = input<string>();

    /** The ID of the conversation, to continue a previous conversation. */
    public readonly conversationId = input(MathHelper.guid());

    /** The ID of the folder to upload images to. */
    public readonly folderId = input<string>();

    /** Defines what is copied when the user uses an answer. */
    public readonly copyMode = input<'Text' | 'Image'>();

    protected readonly chatQuestion = signal('');

    protected readonly chatItems = signal<ReadonlyArray<ChatItem>>([]);

    protected readonly isRunning = signal(false);

    protected readonly user = this.authService.user!;

    constructor() {
        effect(onCleanup => {
            const configuration = this.configuration();
            const conversationId = this.conversationId();

            let observable: Subject<ChatEventDto>;
            const subscription =
                this.translator.ask(this.appsState.appName, { conversationId, configuration })
                    .subscribe(message => {
                        if (message.type === 'History') {
                            const { content, source } = message;

                            this.chatItems.update(s => [...s, { content, type: source }]);
                            return;
                        }

                        if (!observable) {
                            observable = new ReplaySubject<ChatEventDto>();

                            this.chatQuestion.set('');
                            this.chatItems.update(s => [...s, { content: observable, type: 'Assistant' }]);
                            this.isRunning.set(true);
                        }

                        observable.next(message);
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    protected setQuestion(chatQuestion: string) {
        this.chatQuestion.set(chatQuestion);
    }

    protected setCompleted() {
        this.isRunning.set(false);
    }

    protected ask() {
        const prompt = this.chatQuestion();
        if (!prompt || prompt.length === 0) {
            return;
        }

        const stream = this.translator.ask(this.appsState.appName, { prompt, conversationId: this.conversationId(), configuration: this.configuration() });

        this.chatQuestion.set('');
        this.chatItems.update(s => [
            ...s,
            { content: prompt, type: 'User' },
            { content: stream, type: 'Assistant' },
        ]);
        this.isRunning.set(true);
    }
}
