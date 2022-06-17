/************************************************************************************
Copyright : Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Your use of this SDK or tool is subject to the Oculus SDK License Agreement, available at
https://developer.oculus.com/licenses/oculussdk/

Unless required by applicable law or agreed to in writing, the Utilities SDK distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
ANY KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.
************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Oculus.Interaction
{
    /// <summary>
    /// Interactor provides a base template for any kind of interaction.
    /// Interactions can be wholly defined by three things: the concrete Interactor,
    /// the concrete Interactable, and the logic governing their coordination.
    ///
    /// Subclasses are responsible for implementing that coordination logic via template
    /// methods that operate on the concrete interactor and interactable classes.
    /// </summary>
    public abstract class Interactor<TInteractor, TInteractable> : MonoBehaviour, IInteractor
                                    where TInteractor : Interactor<TInteractor, TInteractable>
                                    where TInteractable : Interactable<TInteractor, TInteractable>
    {
        [SerializeField, Interface(typeof(IActiveState)), Optional]
        private MonoBehaviour _activeState;
        private IActiveState ActiveState = null;

        [SerializeField, Interface(typeof(IMonoBehaviourFilter)), Optional]
        private List<MonoBehaviour> _interactableFilters = new List<MonoBehaviour>();
        private List<IMonoBehaviourFilter> InteractableFilters = null;

        protected virtual void DoPreprocess() { }
        protected virtual void DoNormalUpdate() { }
        protected virtual void DoHoverUpdate() { }
        protected virtual void DoSelectUpdate() { }

        public virtual bool ShouldHover
        {
            get => State == InteractorState.Normal && (HasCandidate || ShouldSelect);
            set { }
        }

        public virtual bool ShouldUnhover
        {
            get => State == InteractorState.Hover &&
                   !ShouldSelect &&
                   (_interactable == null || _interactable != _candidate);
            set { }
        }

        public virtual bool ShouldSelect { get; set; }
        public virtual bool ShouldUnselect { get; set; }

        private InteractorState _state = InteractorState.Normal;
        public event Action<InteractorStateChangeArgs> WhenStateChanged = delegate { };
        public event Action WhenPreprocessed = delegate { };
        public event Action WhenPostprocessed = delegate { };

        private ISelector _selector = null;

        public int MaxIterationsPerFrame = 3;

        protected ISelector Selector
        {
            get
            {
                return _selector;
            }
            set
            {
                if (value != _selector)
                {
                    if (_selector != null && _started)
                    {
                        _selector.WhenSelected -= HandleSelected;
                        _selector.WhenUnselected -= HandleUnselected;
                    }
                }

                _selector = value;
                if (_selector != null && _started)
                {
                    _selector.WhenSelected += HandleSelected;
                    _selector.WhenUnselected += HandleUnselected;
                }
            }
        }

        private bool _performSelect = false;
        private bool _performUnselect = false;

        public InteractorState State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (_state == value)
                {
                    return;
                }
                InteractorState previousState = _state;
                _state = value;

                WhenStateChanged(new InteractorStateChangeArgs
                {
                    PreviousState = previousState,
                    NewState = _state
                });
            }
        }

        protected TInteractable _candidate;
        protected TInteractable _interactable;
        protected TInteractable _selectedInteractable;

        public virtual object Candidate => _candidate;

        public TInteractable Interactable => _interactable;
        public TInteractable SelectedInteractable => _selectedInteractable;

        public bool HasCandidate => _candidate != null;
        public bool HasInteractable => _interactable != null;
        public bool HasSelectedInteractable => _selectedInteractable != null;

        private MultiAction<TInteractable> _whenInteractableSet = new MultiAction<TInteractable>();
        private MultiAction<TInteractable> _whenInteractableUnset = new MultiAction<TInteractable>();
        private MultiAction<TInteractable> _whenInteractableSelected = new MultiAction<TInteractable>();
        private MultiAction<TInteractable> _whenInteractableUnselected = new MultiAction<TInteractable>();
        public MAction<TInteractable> WhenInteractableSet => _whenInteractableSet;
        public MAction<TInteractable> WhenInteractableUnset => _whenInteractableUnset;
        public MAction<TInteractable> WhenInteractableSelected => _whenInteractableSelected;
        public MAction<TInteractable> WhenInteractableUnselected => _whenInteractableUnselected;

        protected virtual void InteractableSet(TInteractable interactable)
        {
            _whenInteractableSet.Invoke(interactable);
        }

        protected virtual void InteractableUnset(TInteractable interactable)
        {
            _whenInteractableUnset.Invoke(interactable);
        }

        protected virtual void InteractableSelected(TInteractable interactable)
        {
            _whenInteractableSelected.Invoke(interactable);
        }

        protected virtual void InteractableUnselected(TInteractable interactable)
        {
            _whenInteractableUnselected.Invoke(interactable);
        }

        protected virtual void DoPostprocess() { }

        private UniqueIdentifier _identifier;
        public int Identifier => _identifier.ID;

        protected bool _started;

        protected virtual void Awake()
        {
            _identifier = UniqueIdentifier.Generate();
            ActiveState = _activeState as IActiveState;
            InteractableFilters =
                _interactableFilters.ConvertAll(mono => mono as IMonoBehaviourFilter);
        }

        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            foreach (IMonoBehaviourFilter filter in InteractableFilters)
            {
                Assert.IsNotNull(filter);
            }
            this.EndStart(ref _started);
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                if (_selector != null)
                {
                    _selector.WhenSelected += HandleSelected;
                    _selector.WhenUnselected += HandleUnselected;
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                if (_selector != null)
                {
                    _selector.WhenSelected -= HandleSelected;
                    _selector.WhenUnselected -= HandleUnselected;
                    _performSelect = _performUnselect = false;
                }
                Disable();
            }
        }

        protected virtual void OnDestroy()
        {
            UniqueIdentifier.Release(_identifier);
        }

        public void Preprocess()
        {
            UpdateSelector();
            DoPreprocess();
            if (!UpdateActiveState())
            {
                Disable();
            }
            WhenPreprocessed();
        }

        public void Postprocess()
        {
            DoPostprocess();
            WhenPostprocessed();
        }

        public virtual void UpdateCandidate()
        {
            _candidate = null;
            if (!UpdateActiveState())
            {
                return;
            }
            _candidate = ComputeCandidate();
        }

        public void InteractableChangesUpdate()
        {
            if (_selectedInteractable != null &&
                !_selectedInteractable.HasSelectingInteractor(this as TInteractor))
            {
                UnselectInteractable();
            }

            if (_interactable != null &&
                !_interactable.HasInteractor(this as TInteractor))
            {
                UnsetInteractable();
            }
        }

        public virtual void Select()
        {
            if (State == InteractorState.Select)
            {
                SelectUpdate();
                return;
            }

            if (!ShouldSelect || State != InteractorState.Hover)
            {
                return;
            }

            ShouldSelect = false;

            if (Interactable != null)
            {
                SelectInteractable(Interactable);
            }

            State = InteractorState.Select;
            SelectUpdate();
        }

        public virtual void Unselect()
        {
            if (!ShouldUnselect || State != InteractorState.Select)
            {
                return;
            }

            ShouldUnselect = false;
            UnselectInteractable();
            State = InteractorState.Hover;
        }

        // Returns the best interactable for selection or null
        protected abstract TInteractable ComputeCandidate();

        private void UpdateSelector()
        {
            if (Selector == null)
            {
                return;
            }

            ShouldSelect = _performSelect;
            ShouldUnselect = _performUnselect;

            _performSelect = false;
            _performUnselect = false;
        }

        public virtual bool IsFilterPassedBy(TInteractable interactable)
        {
            if (InteractableFilters == null)
            {
                return true;
            }

            foreach (IMonoBehaviourFilter interactableFilter in InteractableFilters)
            {
                if (!interactableFilter.FilterMonoBehaviour(interactable))
                {
                    return false;
                }
            }
            return true;
        }

        public void Hover()
        {
            if (State == InteractorState.Hover)
            {
                HoverUpdate();
                return;
            }

            if (!ShouldHover || State != InteractorState.Normal)
            {
                return;
            }

            SetInteractable(_candidate);
            State = InteractorState.Hover;
            HoverUpdate();
        }

        public void Unhover()
        {
            if (!ShouldUnhover)
            {
                return;
            }

            UnsetInteractable();
            State = InteractorState.Normal;
            NormalUpdate();
        }

        private void NormalUpdate()
        {
            if (State != InteractorState.Normal)
            {
                return;
            }
            DoNormalUpdate();
        }

        private void HoverUpdate()
        {
            if (State != InteractorState.Hover)
            {
                return;
            }
            DoHoverUpdate();
        }

        private void SelectUpdate()
        {
            if (State != InteractorState.Select)
            {
                return;
            }
            DoSelectUpdate();
        }

        private void SetInteractable(TInteractable interactable)
        {
            if (_interactable == interactable)
            {
                return;
            }
            UnsetInteractable();
            _interactable = interactable;
            interactable.AddInteractor(this as TInteractor);
            InteractableSet(interactable);
        }

        private void UnsetInteractable()
        {
            TInteractable interactable = _interactable;
            if (interactable == null)
            {
                return;
            }
            _interactable = null;
            interactable.RemoveInteractor(this as TInteractor);
            InteractableUnset(interactable);
        }

        private void SelectInteractable(TInteractable interactable)
        {
            Unselect();
            _selectedInteractable = interactable;
            interactable.AddSelectingInteractor(this as TInteractor);
            InteractableSelected(interactable);
        }

        private void UnselectInteractable()
        {
            TInteractable interactable = _selectedInteractable;
            if (interactable == null)
            {
                return;
            }
            _selectedInteractable = null;
            interactable.RemoveSelectingInteractor(this as TInteractor);
            InteractableUnselected(interactable);
        }

        public void Enable()
        {
            if (!UpdateActiveState())
            {
                return;
            }

            if (State == InteractorState.Disabled)
            {
                State = InteractorState.Normal;
            }

            if (State == InteractorState.Normal)
            {
                NormalUpdate();
            }
        }

        public void Disable()
        {
            if (State == InteractorState.Disabled)
            {
                return;
            }

            if (State == InteractorState.Select)
            {
                UnselectInteractable();
                State = InteractorState.Hover;
            }

            if (State == InteractorState.Hover)
            {
                UnsetInteractable();
                State = InteractorState.Normal;
            }

            if (State == InteractorState.Normal)
            {
                State = InteractorState.Disabled;
            }
        }

        protected virtual void HandleSelected()
        {
            _performSelect = true;
        }

        protected virtual void HandleUnselected()
        {
            _performUnselect = true;
        }

        private bool UpdateActiveState()
        {
            if (ActiveState == null || ActiveState.Active)
            {
                return true;
            }
            return false;
        }

        public bool IsRootDriver { get; set; } = true;

        protected virtual void Update()
        {
            if (!IsRootDriver)
            {
                return;
            }

            if (!UpdateActiveState())
            {
                Disable();
                return;
            }

            Preprocess();
            for (int i = 0; i < MaxIterationsPerFrame; i++)
            {
                if (ShouldSelect || State == InteractorState.Select)
                {
                    Select();
                    if (!ShouldUnselect)
                    {
                        break;
                    }
                    Unselect();
                }

                UpdateCandidate();
                Enable();

                if (!ShouldHover && State != InteractorState.Hover)
                {
                    break;
                }

                Hover();

                if (ShouldUnhover)
                {
                    Unhover();
                    break;
                }

                if (!ShouldSelect)
                {
                    break;
                }
            }
            Postprocess();
        }

        #region Inject
        public void InjectOptionalActiveState(IActiveState activeState)
        {
            _activeState = activeState as MonoBehaviour;
            ActiveState = activeState;
        }

        public void InjectOptionalInteractableFilters(List<IMonoBehaviourFilter> interactableFilters)
        {
            InteractableFilters = interactableFilters;
            _interactableFilters = interactableFilters.ConvertAll(interactableFilter =>
                                    interactableFilter as MonoBehaviour);
        }
        #endregion
    }
}
