namespace GateHelper.MysteryTime
{
    partial class MysteryTimeControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.cmbCaseList = new System.Windows.Forms.ComboBox();
            this.lblQuestionCount = new System.Windows.Forms.Label();
            this.txtSituation = new System.Windows.Forms.TextBox();
            this.txtHint = new System.Windows.Forms.TextBox();
            this.rtbChatLog = new System.Windows.Forms.RichTextBox();
            this.txtQuestionInput = new System.Windows.Forms.TextBox();
            this.btnSendQuestion = new System.Windows.Forms.Button();
            this.btnShowHint = new System.Windows.Forms.Button();
            this.btnSubmitAnswer = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.picMainScene = new System.Windows.Forms.PictureBox();
            this.picHintScene = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picMainScene)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHintScene)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbCaseList
            // 
            this.cmbCaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCaseList.FormattingEnabled = true;
            this.cmbCaseList.Location = new System.Drawing.Point(12, 12);
            this.cmbCaseList.Name = "cmbCaseList";
            this.cmbCaseList.Size = new System.Drawing.Size(220, 20);
            this.cmbCaseList.TabIndex = 0;
            this.cmbCaseList.SelectedIndexChanged += new System.EventHandler(this.cmbCaseList_SelectedIndexChanged);
            // 
            // lblQuestionCount
            // 
            this.lblQuestionCount.AutoSize = true;
            this.lblQuestionCount.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblQuestionCount.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblQuestionCount.Location = new System.Drawing.Point(240, 16);
            this.lblQuestionCount.Name = "lblQuestionCount";
            this.lblQuestionCount.Size = new System.Drawing.Size(147, 12);
            this.lblQuestionCount.TabIndex = 1;
            this.lblQuestionCount.Text = "[ 남은 기회: 10 / 10 ]";
            // 
            // btnBack
            // 
            this.btnBack.Location = new System.Drawing.Point(525, 8);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(80, 26);
            this.btnBack.TabIndex = 9;
            this.btnBack.Text = "목록으로";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // txtSituation
            // 
            this.txtSituation.Location = new System.Drawing.Point(12, 42);
            this.txtSituation.Multiline = true;
            this.txtSituation.Name = "txtSituation";
            this.txtSituation.ReadOnly = true;
            this.txtSituation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSituation.Size = new System.Drawing.Size(280, 110);
            this.txtSituation.TabIndex = 2;
            // 
            // txtHint
            // 
            this.txtHint.Location = new System.Drawing.Point(12, 158);
            this.txtHint.Multiline = true;
            this.txtHint.Name = "txtHint";
            this.txtHint.ReadOnly = true;
            this.txtHint.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtHint.Size = new System.Drawing.Size(280, 75);
            this.txtHint.TabIndex = 3;
            // 
            // btnShowHint
            // 
            this.btnShowHint.Location = new System.Drawing.Point(12, 239);
            this.btnShowHint.Name = "btnShowHint";
            this.btnShowHint.Size = new System.Drawing.Size(130, 28);
            this.btnShowHint.TabIndex = 7;
            this.btnShowHint.Text = "💡 힌트보기 (-2회)";
            this.btnShowHint.UseVisualStyleBackColor = true;
            this.btnShowHint.Click += new System.EventHandler(this.btnShowHint_Click);
            // 
            // btnSubmitAnswer
            // 
            this.btnSubmitAnswer.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnSubmitAnswer.ForeColor = System.Drawing.Color.White;
            this.btnSubmitAnswer.Location = new System.Drawing.Point(148, 239);
            this.btnSubmitAnswer.Name = "btnSubmitAnswer";
            this.btnSubmitAnswer.Size = new System.Drawing.Size(144, 28);
            this.btnSubmitAnswer.TabIndex = 8;
            this.btnSubmitAnswer.Text = "🏆 정답 도전";
            this.btnSubmitAnswer.UseVisualStyleBackColor = false;
            this.btnSubmitAnswer.Click += new System.EventHandler(this.btnSubmitAnswer_Click);
            // 
            // picMainScene
            // 
            this.picMainScene.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picMainScene.Location = new System.Drawing.Point(12, 273);
            this.picMainScene.Name = "picMainScene";
            this.picMainScene.Size = new System.Drawing.Size(130, 145);
            this.picMainScene.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picMainScene.TabIndex = 10;
            this.picMainScene.TabStop = false;
            // 
            // picHintScene
            // 
            this.picHintScene.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picHintScene.Location = new System.Drawing.Point(148, 273);
            this.picHintScene.Name = "picHintScene";
            this.picHintScene.Size = new System.Drawing.Size(144, 145);
            this.picHintScene.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picHintScene.TabIndex = 11;
            this.picHintScene.TabStop = false;
            // 
            // rtbChatLog
            // 
            this.rtbChatLog.Location = new System.Drawing.Point(300, 42);
            this.rtbChatLog.Name = "rtbChatLog";
            this.rtbChatLog.ReadOnly = true;
            this.rtbChatLog.Size = new System.Drawing.Size(305, 335);
            this.rtbChatLog.TabIndex = 4;
            this.rtbChatLog.Text = "";
            // 
            // txtQuestionInput
            // 
            this.txtQuestionInput.Location = new System.Drawing.Point(300, 390);
            this.txtQuestionInput.Name = "txtQuestionInput";
            this.txtQuestionInput.Size = new System.Drawing.Size(225, 21);
            this.txtQuestionInput.TabIndex = 5;
            // 
            // btnSendQuestion
            // 
            this.btnSendQuestion.Location = new System.Drawing.Point(530, 387);
            this.btnSendQuestion.Name = "btnSendQuestion";
            this.btnSendQuestion.Size = new System.Drawing.Size(75, 26);
            this.btnSendQuestion.TabIndex = 6;
            this.btnSendQuestion.Text = "질문 전송";
            this.btnSendQuestion.UseVisualStyleBackColor = true;
            this.btnSendQuestion.Click += new System.EventHandler(this.btnSendQuestion_Click);
            // 
            // MysteryTimeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.picHintScene);
            this.Controls.Add(this.picMainScene);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnSubmitAnswer);
            this.Controls.Add(this.btnShowHint);
            this.Controls.Add(this.btnSendQuestion);
            this.Controls.Add(this.txtQuestionInput);
            this.Controls.Add(this.rtbChatLog);
            this.Controls.Add(this.txtHint);
            this.Controls.Add(this.txtSituation);
            this.Controls.Add(this.lblQuestionCount);
            this.Controls.Add(this.cmbCaseList);
            this.Name = "MysteryTimeControl";
            this.Size = new System.Drawing.Size(615, 425);
            this.Load += new System.EventHandler(this.MysteryTimeControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picMainScene)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHintScene)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbCaseList;
        private System.Windows.Forms.Label lblQuestionCount;
        private System.Windows.Forms.TextBox txtSituation;
        private System.Windows.Forms.TextBox txtHint;
        private System.Windows.Forms.RichTextBox rtbChatLog;
        private System.Windows.Forms.TextBox txtQuestionInput;
        private System.Windows.Forms.Button btnSendQuestion;
        private System.Windows.Forms.Button btnShowHint;
        private System.Windows.Forms.Button btnSubmitAnswer;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.PictureBox picMainScene;
        private System.Windows.Forms.PictureBox picHintScene;
    }
}
