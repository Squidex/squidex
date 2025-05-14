/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { DateTime, DynamicFlowDefinitionDto, DynamicFlowStepDefinitionDto, FlowExecutionStateDto, FlowExecutionStepAttemptDto, FlowExecutionStepStateDto, LocalizerService, RuleElementDto, SimulatedRuleEventDto } from '@app/shared';
import { SimulatedRuleEventComponent } from './simulated-rule-event.component';

export default {
    title: 'Rules/SimulatedRuleEvent',
    component: SimulatedRuleEventComponent,
    argTypes: {
        enabled: {
            control: 'boolean',
        },
    },
    render: args => ({
        props: args,
        template: `
            <table class="table table-items table-fixed" style="width: 800px">
                 <tbody [availableSteps]="availableSteps" [expanded]="expanded" [sqxSimulatedRuleEvent]="event"></tbody>
            </table>
        `,
    }),
    decorators: [
        moduleMetadata({
            imports: [
            ],
            providers: [
                {
                    provide: LocalizerService,
                    useValue: new LocalizerService({
                        'common.cancel': 'Cancel',
                        'common.completed': 'Completed',
                        'common.created': 'Created',
                        'common.details': 'Details',
                        'common.error': 'Error',
                        'common.event': 'Event',
                        'common.started': 'Created',
                        'rules.ruleEvents.attempt': 'Attempt',
                        'rules.ruleEvents.attempts': 'Attempts',
                        'rules.ruleEvents.enqueue': 'Enqueue',
                        'rules.ruleEvents.nextAttemptLabel': 'Next',
                        'rules.simulation.actionCreated': 'Job is created from the enriched event and action and added to a job queue.',
                        'rules.simulation.actionExecuted': 'Job is taken from the queue and executed.',
                        'rules.simulation.errorConditionDoesNotMatch': 'STOP: Javascript expressions in trigger do not match to the event.',
                        'rules.simulation.errorConditionPrecheckDoesNotMatch': 'STOP: Condition in trigger does not match to the event.',
                        'rules.simulation.errorDisabled': 'STOP: Rule is disabled.',
                        'rules.simulation.errorFailed': 'Internal Error.',
                        'rules.simulation.errorFromRule': 'STOP: Event has been created from another rule and will be skipped to prevent endless loops.',
                        'rules.simulation.errorNoAction': 'STOP: Action type is obsolete and has been removed.',
                        'rules.simulation.errorNoTrigger': 'STOP: Trigger type is obsolete and has been removed.',
                        'rules.simulation.errorTooOld': 'STOP: Event is too old.',
                        'rules.simulation.errorWrongEvent': 'STOP: Event does not match to the trigger.',
                        'rules.simulation.errorWrongEventForTrigger': 'STOP: Event does not match to the trigger.',
                        'rules.simulation.eventConditionEvaluated': 'Enriched event is evaluated, whether it matchs to the conditions and javascript expressions in the trigger.',
                        'rules.simulation.eventEnriched': 'Event is enriched with additional data',
                        'rules.simulation.eventQueried': 'Event is queried from the database',
                        'rules.simulation.eventTriggerChecked': 'Event is tested to see if it matchs to the trigger and the basic conditions.',
                    }),
                },
            ],
        }),
    ],
} as Meta;

type Story = StoryObj<SimulatedRuleEventComponent>;

const AVAILABLE_STEPS = {
    Delay: new RuleElementDto({
        description: 'Wait a little bit until the next step is executed.',
        display: 'Delay workflow',
        title: 'Delay',
        iconColor: '#3389ff',
        iconImage: '<svg xmlns=\'http://www.w3.org/2000/svg\' viewBox=\'0 0 24 24\'><path d=\'M12.516 6.984v5.25l4.5 2.672-.75 1.266-5.25-3.188v-6h1.5zM12 20.016q3.281 0 5.648-2.367t2.367-5.648-2.367-5.648T12 3.986 6.352 6.353t-2.367 5.648 2.367 5.648T12 20.016zm0-18q4.125 0 7.055 2.93t2.93 7.055-2.93 7.055T12 21.986t-7.055-2.93-2.93-7.055 2.93-7.055T12 2.016z\'/></svg>',
        properties: [],
    }),
    Webhook: new RuleElementDto({
        description: 'Invoke HTTP endpoints on a target system.',
        display: 'Send webhook',
        title: 'Webhook',
        iconColor: '#4bb958',
        iconImage: '<svg xmlns=\'http://www.w3.org/2000/svg\' viewBox=\'0 0 28 28\'><path d=\'M5.95 27.125h-.262C1.75 26.425 0 23.187 0 20.3c0-2.713 1.575-5.688 5.075-6.563V9.712c0-.525.35-.875.875-.875s.875.35.875.875v4.725c0 .438-.35.787-.7.875-2.975.438-4.375 2.8-4.375 4.988s1.313 4.55 4.2 5.075h.175a.907.907 0 0 1 .7 1.05c-.088.438-.438.7-.875.7zM21.175 27.387c-2.8 0-5.775-1.662-6.65-5.075H9.712c-.525 0-.875-.35-.875-.875s.35-.875.875-.875h5.512c.438 0 .787.35.875.7.438 2.975 2.8 4.288 4.988 4.375 2.188 0 4.55-1.313 5.075-4.2v-.088a.908.908 0 0 1 1.05-.7.908.908 0 0 1 .7 1.05v.088c-.612 3.85-3.85 5.6-6.737 5.6zM21.525 18.55c-.525 0-.875-.35-.875-.875v-4.813c0-.438.35-.787.7-.875 2.975-.438 4.288-2.8 4.375-4.987 0-2.188-1.313-4.55-4.2-5.075h-.088c-.525-.175-.875-.613-.787-1.05s.525-.788 1.05-.7h.088c3.938.7 5.688 3.937 5.688 6.825 0 2.713-1.662 5.688-5.075 6.563v4.113c0 .438-.438.875-.875.875zM1.137 6.737H.962c-.438-.087-.788-.525-.7-.963v-.087c.7-3.938 3.85-5.688 6.737-5.688h.087c2.712 0 5.688 1.662 6.563 5.075h4.025c.525 0 .875.35.875.875s-.35.875-.875.875h-4.725c-.438 0-.788-.35-.875-.7-.438-2.975-2.8-4.288-4.988-4.375-2.188 0-4.55 1.313-5.075 4.2v.087c-.088.438-.438.7-.875.7z\'/><path d=\'M7 10.588c-.875 0-1.837-.35-2.538-1.05a3.591 3.591 0 0 1 0-5.075C5.162 3.851 6.037 3.5 7 3.5s1.838.35 2.537 1.05c.7.7 1.05 1.575 1.05 2.537s-.35 1.837-1.05 2.538c-.7.612-1.575.963-2.537.963zM7 5.25c-.438 0-.875.175-1.225.525a1.795 1.795 0 0 0 2.538 2.538c.35-.35.525-.788.525-1.313s-.175-.875-.525-1.225S7.525 5.25 7 5.25zM21.088 23.887a3.65 3.65 0 0 1-2.537-1.05 3.591 3.591 0 0 1 0-5.075c.7-.7 1.575-1.05 2.537-1.05s1.838.35 2.537 1.05c.7.7 1.05 1.575 1.05 2.538s-.35 1.837-1.05 2.537c-.787.7-1.662 1.05-2.537 1.05zm0-5.337c-.525 0-.963.175-1.313.525a1.795 1.795 0 0 0 2.537 2.538c.35-.35.525-.788.525-1.313s-.175-.963-.525-1.313-.787-.438-1.225-.438zM20.387 10.588c-.875 0-1.837-.35-2.537-1.05S16.8 7.963 16.8 7.001s.35-1.837 1.05-2.538c.7-.612 1.662-.962 2.537-.962s1.838.35 2.538 1.05c1.4 1.4 1.4 3.675 0 5.075-.7.612-1.575.963-2.538.963zm0-5.338c-.525 0-.962.175-1.313.525s-.525.788-.525 1.313.175.962.525 1.313c.7.7 1.838.7 2.538 0s.7-1.838 0-2.538c-.263-.438-.7-.612-1.225-.612zM7.087 23.887c-.875 0-1.837-.35-2.538-1.05s-1.05-1.575-1.05-2.537.35-1.838 1.05-2.538c.7-.612 1.575-.962 2.538-.962s1.837.35 2.538 1.05c1.4 1.4 1.4 3.675 0 5.075-.7.612-1.575.962-2.538.962zm0-5.337c-.525 0-.962.175-1.313.525s-.525.788-.525 1.313.175.963.525 1.313a1.794 1.794 0 1 0 2.538-2.537c-.263-.438-.7-.612-1.225-.612z\'/></svg>',
        readMore: 'https://en.wikipedia.org/wiki/Webhook',
        properties: [],
    }),
};

const DEFINITION = new DynamicFlowDefinitionDto({
    initialStepId: '1',
    steps: {
        '1': new DynamicFlowStepDefinitionDto({
            step: { stepType: 'Delay' },
            nextStepId: '2',
        }),
        '2': new DynamicFlowStepDefinitionDto({
            step: { stepType: 'Webhook' },
            nextStepId: null!,
        }),
    },
}) as any;

const now = DateTime.now();

export const Default: Story = {
    args: {
        expanded: true,
        event: new SimulatedRuleEventDto({
            eventName: 'AssetCreated',
            eventId: '1',
            event: {
                type: 'AssetCreated',
            },
            enrichedEvent: {
                type: 'AssetCreated',
            },
            skipReasons: [],
            flowState: new FlowExecutionStateDto({
                context: {
                    type: 'AssetCreated',
                },
                completed: now.addHours(1),
                created: now,
                definition: DEFINITION,
                description: 'AssetCreated',
                nextStepId: null!,
                status: 'Completed',
                steps: {
                    '1': new FlowExecutionStepStateDto({
                        status: 'Completed',
                        attempts: [
                            new FlowExecutionStepAttemptDto({
                                completed: now,
                                started: now,
                                log: [],
                            }),
                            new FlowExecutionStepAttemptDto({
                                completed: now,
                                started: now,
                                log: [],
                            }),
                        ],
                    }),
                    '2': new FlowExecutionStepStateDto({
                        status: 'Completed',
                        attempts: [],
                    }),
                },
            }),
            uniqueId: '1',
        }),
        availableSteps: AVAILABLE_STEPS,
    },
};

export const Failed: Story = {
    args: {
        expanded: true,
        event: new SimulatedRuleEventDto({
            eventName: 'AssetCreated',
            eventId: '1',
            event: {
                type: 'AssetCreated',
            },
            skipReasons: [
                'ConditionDoesNotMatch',
                'ConditionPrecheckDoesNotMatch',
                'Disabled',
                'Failed',
                'FromRule',
                'NoTrigger',
                'TooOld',
                'WrongEvent',
                'WrongEventForTrigger',
            ],
            uniqueId: '1',
        }),
        availableSteps: AVAILABLE_STEPS,
    },
};