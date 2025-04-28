/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { finalize, map, tap } from 'rxjs/operators';
import { debug, DialogService, LoadingState, MathHelper, Mutable, shareSubscribed, State, Types } from '@app/framework';
import { DynamicCreateRuleDto, DynamicFlowDefinitionDto, DynamicFlowStepDefinitionDto, DynamicRuleDto, DynamicUpdateRuleDto, IDynamicFlowStepDefinitionDto } from '../model';
import { RulesService } from '../services/rules.service';
import { AppsState } from './apps.state';

interface Snapshot extends LoadingState {
    // The current rules.
    rules: ReadonlyArray<DynamicRuleDto>;

    // The selected rule.
    selectedRule?: DynamicRuleDto | null;

    // The id of the rule that is currently running.
    runningRuleId?: string;

    // Indicates if a rule run can be cancelled.
    canCancelRun?: boolean;

    // Indicates if the user can create rules.
    canCreate?: boolean;

    // Indicates if the user can read events.
    canReadEvents?: boolean;
}

@Injectable({
    providedIn: 'root',
})
export class RulesState extends State<Snapshot> {
    public selectedRule =
        this.project(x => x.selectedRule);

    public rules =
        this.project(x => x.rules);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public canCancelRun =
        this.project(x => x.canCancelRun === true);

    public canReadEvents =
        this.project(x => x.canReadEvents === true);

    public runningRuleId =
        this.project(x => x.runningRuleId);

    public runningRule =
        this.projectFrom2(this.rules, this.runningRuleId, (r, id) => r.find(x => x.id === id));

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly rulesService: RulesService,
    ) {
        super({ rules: [] });

        debug(this, 'rules');
    }

    public select(id: string | null): Observable<DynamicRuleDto | null> {
        return this.loadIfNotLoaded().pipe(
            map(() => this.snapshot.rules.find(x => x.id === id) || null),
            tap(selectedRule => {
                this.next({ selectedRule });
            }));
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            const { selectedRule } = this.snapshot;

            this.resetState({ selectedRule }, 'Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    public loadIfNotLoaded(): Observable<any> {
        if (this.snapshot.isLoaded) {
            return of({});
        }

        return this.loadInternal(false);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        return this.rulesService.getRules(this.appName).pipe(
            tap(({ items: rules, runningRuleId, canCancelRun, canCreate, canReadEvents }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:rules.reloaded');
                }

                this.next(s => {
                    let selectedRule = s.selectedRule;

                    if (selectedRule) {
                        selectedRule = rules.find(x => x.id === selectedRule!.id);
                    }

                    return {
                        canCancelRun,
                        canCreate,
                        canReadEvents,
                        isLoaded: true,
                        isLoading: false,
                        runningRuleId,
                        rules,
                        selectedRule,
                    };
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: DynamicCreateRuleDto): Observable<DynamicRuleDto> {
        return this.rulesService.postRule(this.appName, request).pipe(
            tap(created => {
                this.next(s => {
                    const rules = [...s.rules, created];

                    return { ...s, rules };
                }, 'Created');
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(rule: DynamicRuleDto): Observable<any> {
        return this.rulesService.deleteRule(this.appName, rule, rule.version).pipe(
            tap(() => {
                this.next(s => {
                    const rules = s.rules.removedBy('id', rule);

                    const selectedRule =
                        s.selectedRule?.id !== rule.id ?
                        s.selectedRule :
                        null;

                    return { ...s, rules, selectedRule };
                }, 'Deleted');
            }),
            shareSubscribed(this.dialogs));
    }

    public update(rule: DynamicRuleDto, request: DynamicUpdateRuleDto): Observable<DynamicRuleDto> {
        return this.rulesService.putRule(this.appName, rule, request, rule.version).pipe(
            tap(updated => {
                this.replaceRule(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public run(rule: DynamicRuleDto): Observable<any> {
        return this.rulesService.runRule(this.appName, rule).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:rules.restarted');
            }),
            shareSubscribed(this.dialogs));
    }

    public runFromSnapshots(rule: DynamicRuleDto): Observable<any> {
        return this.rulesService.runRuleFromSnapshots(this.appName, rule).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:rules.restarted');
            }),
            shareSubscribed(this.dialogs));
    }

    public trigger(rule: DynamicRuleDto): Observable<any> {
        return this.rulesService.triggerRule(this.appName, rule).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:rules.enqueued');
            }),
            shareSubscribed(this.dialogs));
    }

    public runCancel(): Observable<any> {
        return this.rulesService.runCancel(this.appName).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:rules.stop');
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceRule(rule: DynamicRuleDto) {
        this.next(s => {
            const rules = s.rules.replacedBy('id', rule);

            const selectedRule =
                s.selectedRule?.id !== rule.id ?
                s.selectedRule :
                rule;

            return { ...s, rules, selectedRule };
        }, 'Updated');
    }
}

export type BranchItem = { id: string; step: DynamicFlowStepDefinitionDto };
export type BranchList = ReadonlyArray<BranchItem>;

export interface SubBranch {
    // The label for the branch.
    label: string;

    // The actual branch.
    steps: BranchList;

    // The root step ID.
    rootId?: string;

    // The function to set the root.
    setRoot: (id?: string) => void;
}

export class FlowView {
    public readonly mainBranch: BranchList;

    constructor(
        public readonly dto: DynamicFlowDefinitionDto,
        private readonly idGenerator: () => string = () => MathHelper.guid(),
    ) {
        this.mainBranch = getBranch(dto, dto.initialStep);
    }

    public getBranches(parentId?: string): ReadonlyArray<SubBranch> {
        return getBranches(this.dto, this.dto.steps[parentId!]);
    }

    public update(id: string, values: Mutable<IDynamicFlowStepDefinitionDto>) {
        return this.clone(clone => {
            const step = clone.steps[id];
            if (!step) {
                return false;
            }

            clone.steps[id] = new DynamicFlowStepDefinitionDto({ ...values, nextStepId: step.nextStepId });
            return true;
        });
    }

    public add(values: Mutable<IDynamicFlowStepDefinitionDto>, afterId?: string, parentId?: string, branchIndex: number = 0): FlowView {
        if ((parentId && !isIf(this.dto.steps[parentId])) ) {
            return this;
        }

        return this.clone(clone => {
            const parent = clone.steps[parentId!];
            const branches = getBranches(clone, parent);
            const branch = branches[branchIndex];
            if (!branch) {
                return false;
            }

            let afterItem: BranchItem | undefined = undefined;
            if (afterId) {
                afterItem = branch.steps.find(x => x.id === afterId);
                if (!afterItem) {
                    return false;
                }
            }

            const id = this.idGenerator();
            if (!afterItem) {
                values.nextStepId = branch.rootId;
                branch.setRoot(id);
            } else {
                const afterStep = afterItem.step as Mutable<DynamicFlowStepDefinitionDto>;

                values.nextStepId = afterItem.step.nextStepId;
                afterStep.nextStepId = id;
            }

            clone.steps[id] = new DynamicFlowStepDefinitionDto(values);
            return true;
        });
    }

    public remove(id: string, parentId?: string, branchIndex: number = 0): FlowView {
        const step = this.dto.steps[id];
        if (!step || (parentId && !isIf(this.dto.steps[parentId])) ) {
            return this;
        }

        return this.clone(clone => {
            const parent = clone.steps[parentId!];
            const branches = getBranches(clone, parent);
            const branch = branches[branchIndex];
            if (!branch) {
                return false;
            }

            const index = branch.steps.findIndex(x => x.id === id);
            if (index < 0) {
                return false;
            }

            const nextId = step.nextStepId || null!;
            if (branch.rootId === id) {
                branch.setRoot(nextId);
            } else if (index > 0) {
                const step = branch.steps[index - 1].step as Mutable<DynamicFlowStepDefinitionDto>;
                step.nextStepId = nextId;
            }

            delete clone.steps[id];
            return true;
        });
    }

    private clone(action: (clone: Mutable<DynamicFlowDefinitionDto>) => boolean) {
        const clone = DynamicFlowDefinitionDto.fromJSON(this.dto.toJSON());
        if (!action(clone)) {
            return this;
        }
        return new FlowView(cleanup(clone), this.idGenerator);
    }
}

type IfValues = { branches: { condition: string; step?: string }[]; else: string };

function isIf(definition?: DynamicFlowStepDefinitionDto) {
    return definition?.step['stepType'] === 'If';
}

function getBranches(flow: Mutable<DynamicFlowDefinitionDto>, parent?: DynamicFlowStepDefinitionDto): ReadonlyArray<SubBranch> {
    const result: SubBranch[] = [];

    if (!parent || !isIf(parent)) {
        result.push({
            label: 'root',
            steps: getBranch(flow, flow.initialStep),
            setRoot: (id) => {
                flow.initialStep = id!;
            },
            rootId: flow.initialStep,
        });

        return result;
    }

    const { else: elseIf, branches } = parent.step as IfValues;

    if (Types.isArrayOfObject(branches)) {
        for (const branch of branches) {
            result.push({
                label: `if: ${branch.condition}`,
                steps: getBranch(flow, branch.step),
                setRoot: (id) => {
                    branch.step = id;
                },
                rootId: branch.step,
            });
        }
    }

    result.push({
        label: 'else',
        steps: getBranch(flow, elseIf),
        setRoot: (id) => {
            parent.step.else = id;
        },
        rootId: elseIf,
    });

    return result;
}

function getBranch(flow: Mutable<DynamicFlowDefinitionDto>, initialStep?: string): BranchList {
    const result: BranchItem[] = [];

    let stepId: string | undefined = initialStep;
    while (stepId && stepId !== MathHelper.EMPTY_GUID) {
        const step = flow.steps[stepId];
        if (!step) {
            break;
        }

        result.push({ id: stepId, step });
        stepId = step.nextStepId;
    }

    return result;
}

function cleanup(dto: DynamicFlowDefinitionDto) {
    const ids = new Set<string | null | undefined>([dto.initialStep]);

    for (const item of Object.values(dto.steps)) {
        ids.add(item.nextStepId);

        if (isIf(item)) {
            const { else: elseIf, branches } = item.step as IfValues;
            if (Types.isArrayOfObject(branches)) {
                for (const branch of branches) {
                    ids.add(branch.step);
                }
            }

            ids.add(elseIf);
        }
    }

    for (const id of Object.keys(dto.steps)) {
        if (!ids.has(id)) {
            delete dto.steps[id];
        }
    }

    return dto;
}