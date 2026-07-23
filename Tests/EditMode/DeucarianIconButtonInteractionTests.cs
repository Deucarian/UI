using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianIconButtonInteractionTests
    {
        [Test]
        public void Bind_TracksHoverStateAndNotifiesChanges()
        {
            Button button = new Button();
            TestWindow window = EditorWindow.CreateWindow<TestWindow>();
            window.rootVisualElement.Add(button);
            DeucarianIconButtonInteraction interaction = new DeucarianIconButtonInteraction();
            int changeCount = 0;
            try
            {
                interaction.Bind(button, () => changeCount++);

                SendMouseEnter(button);

                Assert.True(interaction.Hovered);
                Assert.AreEqual(1, changeCount);

                SendMouseLeave(button);

                Assert.False(interaction.Hovered);
                Assert.False(interaction.Pressed);
                Assert.AreEqual(2, changeCount);
            }
            finally
            {
                interaction.Dispose();
                window.Close();
            }
        }

        [Test]
        public void Unbind_RemovesCallbacksResetsStateAndIsIdempotent()
        {
            Button button = new Button();
            TestWindow window = EditorWindow.CreateWindow<TestWindow>();
            window.rootVisualElement.Add(button);
            DeucarianIconButtonInteraction interaction = new DeucarianIconButtonInteraction();
            int changeCount = 0;
            try
            {
                interaction.Bind(button, () => changeCount++);
                SendMouseEnter(button);

                interaction.Unbind();
                interaction.Dispose();
                interaction.Dispose();
                SendMouseEnter(button);

                Assert.False(interaction.Hovered);
                Assert.False(interaction.Pressed);
                Assert.False(interaction.Focused);
                Assert.AreEqual(1, changeCount);
            }
            finally
            {
                interaction.Dispose();
                window.Close();
            }
        }

        private static void SendMouseEnter(VisualElement element)
        {
            using (MouseEnterEvent evt = MouseEnterEvent.GetPooled())
            {
                evt.target = element;
                element.SendEvent(evt);
            }
        }

        private static void SendMouseLeave(VisualElement element)
        {
            using (MouseLeaveEvent evt = MouseLeaveEvent.GetPooled())
            {
                evt.target = element;
                element.SendEvent(evt);
            }
        }

        private sealed class TestWindow : EditorWindow
        {
        }
    }
}
