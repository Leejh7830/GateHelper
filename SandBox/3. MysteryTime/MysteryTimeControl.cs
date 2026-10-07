using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using GateHelper.MysteryTime;

namespace GateHelper.MysteryTime
{
    public partial class MysteryTimeControl : UserControl
    {
        private MysteryCaseRepository _repository;
        private MysteryAiEngine _aiEngine;
        private MysteryCase _currentCase;
        private int _remainingQuestions = 10;
        private int _currentQuestionNumber = 0;
        private bool _isHintRevealed = false;

        public MysteryTimeControl()
        {
            InitializeComponent();
            _repository = new MysteryCaseRepository();
            _aiEngine = new MysteryAiEngine(GetApiKeyFromConfig());
        }

        private string GetApiKeyFromConfig()
        {
            try
            {
                return Util_Option.LoadUserOptions()?.GeminiApiKey ?? "";
            }
            catch
            {
                return "";
            }
        }

        private void MysteryTimeControl_Load(object sender, EventArgs e)
        {
            LoadPresetCasesCombo();
        }

        public void LoadPresetCasesCombo()
        {
            try
            {
                cmbCaseList.Items.Clear();
                var cases = _repository.GetAllCases();
                foreach (var c in cases)
                {
                    cmbCaseList.Items.Add($"[{c.Difficulty}] {c.Title}");
                }

                if (cmbCaseList.Items.Count > 0)
                {
                    cmbCaseList.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void cmbCaseList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = cmbCaseList.SelectedIndex;
            var cases = _repository.GetAllCases();
            if (idx >= 0 && idx < cases.Count)
            {
                StartNewCase(cases[idx]);
            }
        }

        public void StartNewCase(MysteryCase targetCase)
        {
            _currentCase = targetCase;
            _remainingQuestions = 10;
            _currentQuestionNumber = 0;
            _isHintRevealed = false;

            if (lblQuestionCount != null)
                lblQuestionCount.Text = $"[ 남은 기회: {_remainingQuestions} / 10 ]";

            if (txtSituation != null)
                txtSituation.Text = _currentCase.Situation;

            if (txtHint != null)
                txtHint.Text = "💡 [힌트 보기]는 질문 5회 이상 진행 후 사용할 수 있습니다.";

            if (rtbChatLog != null)
            {
                rtbChatLog.Clear();
                AppendChatLog("🤖 [AI 진행자]", $"사건 '{_currentCase.Title}'이 시작되었습니다. 사건의 전말을 추리하기 위해 '예/아니오' 질문을 작성해 주세요.", Color.DarkBlue);
            }

            if (picHintScene != null)
                picHintScene.Image = null;
        }

        private async void btnSendQuestion_Click(object sender, EventArgs e)
        {
            await ProcessUserQuestionAsync();
        }

        private async Task ProcessUserQuestionAsync()
        {
            if (_currentCase == null) return;

            string questionText = txtQuestionInput.Text.Trim();
            if (string.IsNullOrEmpty(questionText)) return;

            if (_remainingQuestions <= 0)
            {
                MessageBox.Show("남은 질문 기회를 모두 사용하셨습니다! [사건의 전말 제출]을 도전해보세요.", "기회 소진", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtQuestionInput.Clear();
            _currentQuestionNumber++;
            _remainingQuestions--;

            lblQuestionCount.Text = $"[ 남은 기회: {_remainingQuestions} / 10 ]";
            AppendChatLog($"Q{_currentQuestionNumber}. 유저", questionText, Color.Black);

            btnSendQuestion.Enabled = false;
            try
            {
                var response = await _aiEngine.EvaluateQuestionAsync(_currentCase, questionText, _currentQuestionNumber);
                
                Color ansColor = Color.DarkGreen;
                if (response.AnswerType == MysteryAnswerType.No) ansColor = Color.DarkRed;
                else if (response.AnswerType == MysteryAnswerType.Ambiguous) ansColor = Color.OrangeRed;
                else if (response.AnswerType == MysteryAnswerType.Irrelevant) ansColor = Color.Gray;

                AppendChatLog("🤖 [AI 진행자]", response.AnswerMessage, ansColor);
            }
            catch (Exception ex)
            {
                AppendChatLog("🤖 [AI 진행자]", $"오류 발생: {ex.Message}", Color.Red);
            }
            finally
            {
                btnSendQuestion.Enabled = true;
            }
        }

        private async void btnSubmitAnswer_Click(object sender, EventArgs e)
        {
            if (_currentCase == null) return;

            string solutionText = txtQuestionInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(solutionText))
            {
                MessageBox.Show("하단 질문 입력창에 추리한 사건의 전말(정답)을 적은 후 [🏆 정답 도전] 버튼을 눌러주세요!", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnSubmitAnswer.Enabled = false;
            try
            {
                var result = await _aiEngine.EvaluateAnswerAsync(_currentCase, solutionText);
                
                string titleStr = result.isCorrect ? "🎉 정답입니다!" : "❌ 아쉬운 오답입니다.";
                MessageBoxIcon iconStr = result.isCorrect ? MessageBoxIcon.Information : MessageBoxIcon.Warning;

                MessageBox.Show(result.feedback, titleStr, MessageBoxButtons.OK, iconStr);

                if (result.isCorrect)
                {
                    AppendChatLog("🏆 [축하합니다!]", $"정답을 맞히셨습니다!\n{result.feedback}", Color.Blue);
                }

                txtQuestionInput.Clear();
            }
            finally
            {
                btnSubmitAnswer.Enabled = true;
            }
        }

        private void btnShowHint_Click(object sender, EventArgs e)
        {
            if (_currentCase == null) return;

            // 1. 이미 힌트가 공개된 경우 (메시지 팝업 없이 차감 없이 기존 힌트만 재출력)
            if (_isHintRevealed)
            {
                txtHint.Text = $"💡 [힌트 공개]: {_currentCase.HintText}";
                AppendChatLog("💡 [힌트 공개]", _currentCase.HintText, Color.DarkOrange);
                return;
            }

            // 2. 질문 5회 미만 진행 시 힌트 사용 불가
            if (_currentQuestionNumber < 5)
            {
                MessageBox.Show($"힌트는 질문을 최소 5회 이상 진행한 후 사용할 수 있습니다.\n(현재 진행 질문: {_currentQuestionNumber} / 5회)", "힌트 사용 제한", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 3. 기회 부족 시 (최소 2개 필요)
            if (_remainingQuestions < 2)
            {
                MessageBox.Show("힌트를 보려면 남은 질문 기회가 최소 2개 이상 필요합니다.", "기회 부족", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _isHintRevealed = true;
            _remainingQuestions -= 2;
            lblQuestionCount.Text = $"[ 남은 기회: {_remainingQuestions} / 10 ]";

            txtHint.Text = $"💡 [힌트 공개]: {_currentCase.HintText}";
            AppendChatLog("💡 [힌트 공개]", _currentCase.HintText, Color.DarkOrange);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (this.ParentForm is SandBox sb)
            {
                sb.ShowGameList();
            }
        }

        private void AppendChatLog(string senderName, string message, Color color)
        {
            if (rtbChatLog == null) return;

            rtbChatLog.SelectionStart = rtbChatLog.TextLength;
            rtbChatLog.SelectionLength = 0;

            rtbChatLog.SelectionColor = color;
            rtbChatLog.SelectionFont = new Font(rtbChatLog.Font, FontStyle.Bold);
            rtbChatLog.AppendText($"{senderName}: ");

            rtbChatLog.SelectionColor = Color.Black;
            rtbChatLog.SelectionFont = new Font(rtbChatLog.Font, FontStyle.Regular);
            rtbChatLog.AppendText($"{message}\n\n");

            rtbChatLog.ScrollToCaret();
        }
    }
}
