/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, onErrorResumeNextWith, throwError } from 'rxjs';
import { TestValues } from 'src/app/shared/state/_test-helpers';
import { IMock, It, Mock, Times } from 'typemoq';
import { DialogService, PlanChangedDto, PlanDto, PlansDto, versioned } from '@app/shared';
import { TeamPlansService, TeamPlansState } from '../internal';

describe('TeamPlansState', () => {
    const {
        creator,
        newVersion,
        team,
        teamsState,
        version,
    } = TestValues;

    const oldPlans = new PlansDto({
        currentPlanId: 'id1',
        planOwner: creator,
        plans: [
            new PlanDto({
                id: 'free',
                name: 'Free',
                costs: '14 €',
                confirmText: 'Change for 14 € per month?',
                yearlyId: 'free_yearly',
                yearlyCosts: '120 €',
                yearlyConfirmText: 'Change for 120 € per year?',
                maxApiBytes: 128,
                maxApiCalls: 1000,
                maxAssetSize: 1500,
                maxContributors: 2500,
            }),
            new PlanDto({
                id: 'professional',
                name: 'Professional',
                costs: '18 €',
                confirmText: 'Change for 18 € per month?',
                yearlyId: 'professional_yearly',
                yearlyCosts: '160 €',
                yearlyConfirmText: 'Change for 160 € per year?',
                maxApiBytes: 512,
                maxApiCalls: 4000,
                maxAssetSize: 5500,
                maxContributors: 6500,
            }),
        ],
        locked: 'None',
    });

    let dialogs: IMock<DialogService>;
    let plansService: IMock<TeamPlansService>;
    let plansState: TeamPlansState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        plansService = Mock.ofType<TeamPlansService>();
        plansState = new TeamPlansState(teamsState.object, dialogs.object, plansService.object);
    });

    afterEach(() => {
        plansService.verifyAll();
    });

    describe('Loading', () => {
        it('should load plans', () => {
            plansService.setup(x => x.getPlans(team))
                .returns(() => of(versioned(version, oldPlans))).verifiable();

            plansState.load().subscribe();

            expect(plansState.snapshot.plans).toEqual([
                { isSelected: true, isYearlySelected: false, plan: oldPlans.plans[0] },
                { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[1] },
            ]);
            expect(plansState.snapshot.isLoaded).toBeTruthy();
            expect(plansState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should load plans with overriden id', () => {
            plansService.setup(x => x.getPlans(team))
                .returns(() => of(versioned(version, oldPlans))).verifiable();

            plansState.load(false, 'id2_yearly').subscribe();

            expect(plansState.snapshot.plans).toEqual([
                { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[0] },
                { isSelected: false, isYearlySelected: true, plan: oldPlans.plans[1] },
            ]);
            expect(plansState.snapshot.isLoaded).toBeTruthy();
            expect(plansState.snapshot.isLoading).toBeFalsy();
            expect(plansState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            plansService.setup(x => x.getPlans(team))
                .returns(() => throwError(() => 'Service Error'));

            plansState.load().pipe(onErrorResumeNextWith()).subscribe();

            expect(plansState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            plansService.setup(x => x.getPlans(team))
                .returns(() => of(versioned(version, oldPlans))).verifiable();

            plansState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            plansState.window = <any>{ location: {} };

            plansService.setup(x => x.getPlans(team))
                .returns(() => of(versioned(version, oldPlans))).verifiable();

            plansState.load().subscribe();
        });

        it('should redirect if returning url', () => {
            plansState.window = <any>{ location: {} };

            const result = { redirectUri: 'http://url' };

            plansService.setup(x => x.putPlan(team, It.isAny(), version))
                .returns(() => of(versioned(newVersion, new PlanChangedDto(result))));

            plansState.change('free').pipe(onErrorResumeNextWith()).subscribe();

            expect(plansState.snapshot.plans).toEqual([
                { isSelected: true, isYearlySelected: false, plan: oldPlans.plans[0] },
                { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[1] },
            ]);
            expect(plansState.window.location.href).toBe(result.redirectUri);
            expect(plansState.snapshot.version).toEqual(version);
        });

        it('should update plans if no returning url', () => {
            plansService.setup(x => x.putPlan(team, It.isAny(), version))
                .returns(() => of(versioned(newVersion, new PlanChangedDto())));

            plansState.change('id2_yearly').pipe(onErrorResumeNextWith()).subscribe();

            expect(plansState.snapshot.plans).toEqual([
                { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[0] },
                { isSelected: false, isYearlySelected: true, plan: oldPlans.plans[1] },
            ]);
            expect(plansState.snapshot.version).toEqual(newVersion);
        });
    });
});
